using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Datacom.IRIS.Common.Helpers;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Datacom.IRIS.Common.Utils.EntityFramework.ModelMerging
{
    /// <summary>Merge Model(.EDMX file) metadata to allow multiple models to share the same context via the connection string.</summary>
    public class ModelMergerService
    {
        //Some code from http://multifileedmx.codeplex.com and some from http://www.hendryten.com/tag/edmx/ thx.
        //For background information on the purpose of this see http://simonburgoyne.com/blog/?p=347

        #region Declarations

        public const string MergeMessageNoModelsToMerge = "No Models to merge";

        /// <summary>File extensions for the three metadata model files</summary>
        private const string DatastoreModelFileExtention = "ssdl";
        private const string ConceptualModelFileExtension = "csdl";
        private const string MappingModelFileExtension = "msl";
        private const string ModelFileExtension = "edmx";
        private const string NoDesignerFilePart = ".NoDesigner";
        //private readonly static string DiagramsModelFileExtension = "Designer.diagrams";

        private List<FileInfo> ModelFileInfos { get; set; }
        public IModelMergerXmlService ModelMergerXmlService { get; set; }
        public IModelMergerTFSService ModelMergerTFSService  { get; set; }

        private List<string> _concurrencyWarningExclusions;
        public List<string> ConcurrencyWarningExclusions { get { return _concurrencyWarningExclusions ?? (_concurrencyWarningExclusions = new List<string>()); } } 

        #endregion

        #region Constructors

        /// <summary>Model merging service requires a list of models to merge.</summary>
        /// <param name="modelsToMerge">List of model file information</param>
        private ModelMergerService(IEnumerable<FileInfo> modelsToMerge, IModelMergerXmlService modelMergerXmlService, IModelMergerTFSService modelMergerTFSService)
        {
            ModelFileInfos = modelsToMerge.ToList();
            ModelMergerTFSService = modelMergerTFSService;
            ModelMergerXmlService = modelMergerXmlService;
        }

        public static ModelMergerService New(IEnumerable<FileInfo> modelsToMerge)
        {
            return new ModelMergerService(modelsToMerge, new ModelMergerXmlService(), new ModelMergerTFSService());
        }

        /// <summary>Static constructor for the model merging service requires a list of models to merge.</summary>
        /// <param name="modelsToMerge">List of model file information</param>
        /// <param name="modelMergerXmlService">Service that manages the schema xml</param>
        /// <param name="modelMergerTFSService">Service that manages TFS operations</param>
        /// <returns>A model merging service</returns>
        public static ModelMergerService New(IEnumerable<FileInfo> modelsToMerge, IModelMergerXmlService modelMergerXmlService, IModelMergerTFSService modelMergerTFSService)
        {
            return new ModelMergerService(modelsToMerge,modelMergerXmlService,modelMergerTFSService);
        } 

        #endregion

        /// <summary>Merge the models</summary>
        /// <param name="outputPath">path to save the merged model metadata files</param>
        /// <param name="mergeFileName">file name to use for the merged files</param>
        /// <param name="nameSpace">Schema namespace</param>
        /// <returns>Information on the merge including output file paths and if the merge was successful.</returns>
        public MergedModelInfo MergeModels(string outputPath, string mergeFileName = "mergedSchema")
        {
            var result = new MergedModelInfo();
            SetOutputFilePaths(outputPath, mergeFileName, result);

            if (NoModels(result)) {return result;}

            ValidateEDMXFiles(result);

            XDocument csdlDoc = null, ssdlDoc = null, mslDoc = null, modelDoc = null;
            MergeModelFileSchemas(ref csdlDoc, ref ssdlDoc, ref mslDoc);
            modelDoc = GenerateEDMXModel(csdlDoc, ssdlDoc, mslDoc);

            SaveMergedFiles(outputPath, result, csdlDoc, ssdlDoc, mslDoc, modelDoc);

            result.Result = true;

            return result;
        }

        /// <summary>
        ///  Validate the original EDMX files before merging them. Our validation won't
        ///  stop the merge process if any validation error or warning found, but we 
        ///  keep these errors and warnings in the merge report. The developer should
        ///  review the report and correct the errors whenever they merge the EDMX files.
        /// 
        ///  IRIS USAGE: The method currently do two validation for IRIS
        ///     1. Concurrency setting checking: In IRIS, any parent level entity (has 
        ///     LastModify property) should set the "ConcurrencyMode" = "Fixed" in order
        ///     to obtain the EntityFramework build-in concurrency checking while saving
        ///     the entity changes.
        ///     2. Duplicated entities in different model: Any entity should only be 
        ///     defined once in all of the EDMX files. The same entity which used in 
        ///     other EDMX files as the reference entity should marked as "ModelLink" tag.
        ///  NOTE: This method is independent from other merging process, so can be token
        ///  out without any effect of other processes. It can be separated as a validation
        ///  class as well.
        /// </summary>
        /// <param name="mergedModelInfo"></param>
        private void ValidateEDMXFiles(MergedModelInfo mergedModelInfo)
        {
            Dictionary<string, KeyValuePair<string, XElement>> checkedEntityTypeElements = new Dictionary<string, KeyValuePair<string, XElement>>();

            // for each EDMX file
            foreach (var modelFileInfo in ModelFileInfos)
            {
                var modelSchema = ModelMergerXmlService.LoadSchema(modelFileInfo);
                if (modelSchema == null) return;

                if (modelSchema.HasConceptualSchema)
                {
                    XDocument csdlDoc = new XDocument(modelSchema.ConceptualSchema);
                    XElement lSchema = csdlDoc.Elements().Where(el => el.Name.LocalName == "Schema").FirstOrDefault();
                    if (lSchema == null) return;

                    var lNameSpace = XmlHelper.GetNameAttribute(lSchema, "Namespace");
                    if (String.IsNullOrWhiteSpace(lNameSpace)) return;

                    var elEntityTypes = lSchema.Elements().Where(el => el.Name.LocalName == "EntityType");

                    // for each entity inside an EDMX file
                    foreach (var elEntityType in elEntityTypes)
                    {
                        string sName = XmlHelper.GetNameAttribute(elEntityType);
                        if (String.IsNullOrWhiteSpace(sName)) continue;

                        KeyValuePair<string, XElement> elEntityTypeUsed;
                        // If first time found the entity without "ModelLink" tag, we do the ConcurrencySetting
                        // check and add it into our checked list.
                        if (!checkedEntityTypeElements.TryGetValue(sName, out elEntityTypeUsed))
                        {
                            if (elEntityType.Descendants().Where(el => el.Value == "ModelLink").Count() == 0)
                            {
                                ValidateConcurrencySetting(elEntityType, mergedModelInfo, modelFileInfo.Name);
                                checkedEntityTypeElements.Add(sName, new KeyValuePair<string, XElement>(modelFileInfo.Name, elEntityType));
                            }
                        }
                        // if found an entity twice without "ModelLink" tag, raise a warning
                        else if (elEntityType.Descendants().Where(el => el.Value == "ModelLink").Count() == 0)
                        {
                            mergedModelInfo.ValidationWarningMessages.Add(new KeyValuePair<string, string>("DuplicatedEntity", string.Format("Please mark one of the {0} entities to be 'ModelLink' in either {1} or {2}. The merge process ignored the one in {2}.", sName, elEntityTypeUsed.Key, modelFileInfo.Name)));
                        }
                    }
                }
            }
            checkedEntityTypeElements.Clear();
        }

        /// <summary>
        /// Concurrency setting checking: In IRIS, any parent level entity (has 
        /// LastModified property) should set the LastModified.ConcurrencyMode = "Fixed" 
        /// in order to obtain the EntityFramework build-in concurrency checking while 
        /// saving entity changes.
        /// </summary>
        /// <param name="csdlEntity"></param>
        /// <param name="mergedModelInfo"></param>
        /// <param name="modelName"></param>
        private void ValidateConcurrencySetting(XElement csdlEntity, MergedModelInfo mergedModelInfo, string modelName)
        {
            IEnumerable<XElement> propertyElements = csdlEntity.Descendants().Where(el => el.Name.LocalName == "Property");
            foreach (var property in propertyElements)
            {
                if (XmlHelper.GetNameAttribute(property) == "LastModified")
                {
                    XAttribute concurrencyAttribute = property.Attribute("ConcurrencyMode");
                    if (concurrencyAttribute == null || concurrencyAttribute.Value != "Fixed")
                    {
                        string entityName = XmlHelper.GetNameAttribute(csdlEntity);
                        if (! ConcurrencyWarningExclusions.Contains(entityName))
                            mergedModelInfo.ValidationWarningMessages.Add(new KeyValuePair<string, string>("ConcurrencySetting", string.Format("The concurrency setting is incorrect. EDMX file:{0}, Entity:{1}. \t Please set LastModified.ConcurrencyMode='Fixed'.", modelName, entityName)));
                    }
                }
            }
        }

        private bool NoModels(MergedModelInfo result)
        {
            if (ModelFileInfos.Count == 0)
            {
                result.Message = MergeMessageNoModelsToMerge;
                return true;
            }
            return false;
        }
        private void MergeModelFileSchemas(ref XDocument csdlDoc, ref XDocument ssdlDoc, ref XDocument mslDoc)
        {
            var navigationProperties = new Dictionary<string, List<XElement>>();
            foreach (var modelFileInfo in ModelFileInfos)
            {
                var modelSchema = ModelMergerXmlService.LoadSchema(modelFileInfo);
                if (modelSchema == null) return;

                var schemaNamespace = "";
                
                if (modelSchema.HasConceptualSchema)
                {
                    if (csdlDoc == null)
                        csdlDoc = new XDocument(modelSchema.ConceptualSchema);
                    else
                        new SdlMerger(csdlDoc).Merge(new XDocument(modelSchema.ConceptualSchema), out schemaNamespace, navigationProperties);
                }

                if (modelSchema.HasStorageSchema)
                {
                    if (ssdlDoc == null)
                        ssdlDoc = new XDocument(modelSchema.StorageSchema);
                    else
                        new SdlMerger(ssdlDoc).Merge(new XDocument(modelSchema.StorageSchema));
                }

                if (modelSchema.HasMappingSchema)
                {
                    if (mslDoc == null)
                        mslDoc = new XDocument(modelSchema.MappingSchema);
                    else
                        new MslMerger(mslDoc, schemaNamespace).Merge(new XDocument(modelSchema.MappingSchema),
                                                                     schemaNamespace);
                }
            }
        }

        private XDocument GenerateEDMXModel(XDocument csdlDoc, XDocument ssdlDoc, XDocument mslDoc)
        {
            var modelXmlService = new ModelXmlServiceEntityFrameworkv4R0(ModelMergerXmlService.GetModelSkeletonAsDocument())
                                      {
                                          StorageSchema = ssdlDoc.Root,
                                          MappingSchema = mslDoc.Root,
                                          ConceptualSchema = csdlDoc.Root
                                      };
            return modelXmlService.ModelDocument;
        }
        private static void SetOutputFilePaths(string outputPath, string mergeFileName, MergedModelInfo result)
        {
            var filePathWithoutExtension = Path.Combine(outputPath, mergeFileName);
            result.ConceptualSchemaFilePath = Path.ChangeExtension(filePathWithoutExtension, ConceptualModelFileExtension);
            result.DatastoreSchemaFilePath = Path.ChangeExtension(filePathWithoutExtension, DatastoreModelFileExtention);
            result.MappingSchemaFilePath = Path.ChangeExtension(filePathWithoutExtension, MappingModelFileExtension);
            result.ModelFilePath = filePathWithoutExtension + NoDesignerFilePart + "." + ModelFileExtension;
        }
        private void SaveMergedFiles(string outputPath, MergedModelInfo result, XDocument csdlDoc, XDocument ssdlDoc, XDocument mslDoc, XDocument modelDoc)
        {
            CreateTheOutputDirectoryIfItDoesNotExist(outputPath);
            try
            {
                CheckoutOutputFiles(outputPath, result);
            }
            catch (Exception exception)
            {
                result.Message = string.Format("Filed to checkout metadata files {0}, {1}, {2}. Error: {3}",
                                               ConceptualModelFileExtension, DatastoreModelFileExtention,
                                               MappingModelFileExtension, exception.Message);
            }

            ModelMergerXmlService.SaveSchema(csdlDoc, result.ConceptualSchemaFilePath, SaveOptions.None);
            ModelMergerXmlService.SaveSchema(ssdlDoc, result.DatastoreSchemaFilePath, SaveOptions.None);
            ModelMergerXmlService.SaveSchema(mslDoc, result.MappingSchemaFilePath, SaveOptions.None);
            ModelMergerXmlService.SaveSchema(modelDoc, result.ModelFilePath, SaveOptions.None);
        }

        private void CheckoutOutputFiles(string outputPath, MergedModelInfo result)
        {
            var filePaths = new List<string>();
            
            if (result.ConceptualSchemaFilePath != null) filePaths.Add(result.ConceptualSchemaFilePath);
            if (result.DatastoreSchemaFilePath != null) filePaths.Add(result.DatastoreSchemaFilePath);
            if (result.MappingSchemaFilePath != null) filePaths.Add(result.MappingSchemaFilePath);
            if (result.ModelFilePath != null) filePaths.Add(result.ModelFilePath);

            ModelMergerTFSService.SetWorkspacePath(outputPath);
            
            foreach (var filePath in filePaths)
            {
                ModelMergerTFSService.Checkout(filePath);
            }
        }
        private static void CreateTheOutputDirectoryIfItDoesNotExist(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }
    }

    // TODO refactor mapping classes to methods in the service will do here for now.
    // Initial code from http://www.hendryten.com/tag/edmx/ thx.

    /// <summary>Merge mapping specification Language (MSL) xml files</summary>   
    internal class MslMerger
    {
        public XDocument Document { get; private set; }
        public string BaseNamespace { get; private set; }

        public MslMerger(XDocument xDocument, string sNamespace)
        {
            Document = xDocument;
            BaseNamespace = sNamespace;

            Initialize();
        }

        /// <summary>
        /// Merge with the base MSL
        /// </summary>
        /// <param name="xDocument">XDocument intended to be merged</param>
        /// <param name="sMergedNamespace">Namespace of the merged XDocument</param>
        public void Merge(XDocument xDocument, string sMergedNamespace)
        {
            var elContainer = xDocument.Descendants().Where(el => el.Name.LocalName == "EntityContainerMapping").FirstOrDefault();
            if (elContainer == null)
                throw new Exception("Invalid msl data");

            var elSet = elContainer.Elements();
            foreach (var el in elSet)
            {
                var attrName = el.Name.LocalName == "FunctionImportMapping" ? "FunctionImportName" : "Name";
                var name = XmlHelper.GetNameAttribute(el, attrName);
                if (!_mElements.ContainsKey(name))
                {
                    _mElements.Add(name, el);
                    _mContainer.Add(el);
                }
            }
        }

        private XElement _mContainer;
        private readonly Dictionary<string, XElement> _mElements = new Dictionary<string, XElement>();

        private void Initialize()
        {
            var elContainer = Document.Descendants().Where(el => el.Name.LocalName == "EntityContainerMapping").FirstOrDefault();
            if (elContainer == null)
                throw new Exception("Invalid msl data");

            _mContainer = elContainer;

            var elSet = elContainer.Elements();
            foreach (var el in elSet)
            {
                var attrName = el.Name.LocalName == "FunctionImportMapping" ? "FunctionImportName" : "Name";
                var name = XmlHelper.GetNameAttribute(el, attrName);
                _mElements.Add(name, el);
            }
        }
    }

    /// <summary>Merge Schema Definition Language (C/S SDL) xml files</summary>   
    internal class SdlMerger
    {
        /// <summary>XDocument of the Base (C/S)SDL</summary>
        public XDocument Document { get; private set; }

        /// <summary> Namespace of the schema </summary>
        public string SchemaNamespace { get; private set; }

        public SdlMerger(XDocument baseDocument)
        {
            Document = baseDocument;
            Initialize();
        }

        /// <summary>Merge an (C/S)SDL XDocument with the base document</summary>
        /// <param name="xDocument">XDocument intended to be merged</param>
        public void Merge(XDocument xDocument)
        {
            string sName;
            Merge(xDocument, out sName, new Dictionary<string, List<XElement>>());
        }

        /// <summary>Merge an (C/S)SDL XDocument with the base document</summary>
        /// <param name="xDocument">XDocument intended to be merged</param>
        /// <param name="sNamespace">Namespace of the merged (C/S)SDL</param>
        public void Merge(XDocument xDocument, out string sNamespace, Dictionary<string,List<XElement>> navigationProperties)
        {
            sNamespace = null;
            XElement lSchema = xDocument.Elements().Where(el => el.Name.LocalName == "Schema").FirstOrDefault();
            if (lSchema == null) throw new InvalidOperationException("Invalid sdl data");

            var lNameSpace = XmlHelper.GetNameAttribute(lSchema, "Namespace");
            if (String.IsNullOrWhiteSpace(lNameSpace))
                throw new InvalidOperationException("Invalid sdl data");
            sNamespace = lNameSpace;

            var lContainer = lSchema.Elements().Where(el => el.Name.LocalName == "EntityContainer").FirstOrDefault();
            if (lContainer != null)
            {
                var elContainer = lContainer.Elements();
                foreach (var elItemContainer in elContainer)
                {
                    var sName = XmlHelper.GetNameAttribute(elItemContainer);
                    if (_mInContainerSetElements.ContainsKey(sName)) continue;
                    _mInContainerSetElements.Add(sName, elItemContainer);
                    _mEntityContainerElement.Add(elItemContainer);
                }
            }

            var elEntityTypes = lSchema.Elements().Where(el => el.Name.LocalName == "EntityType");
            foreach (var elEntityType in elEntityTypes)
            {
                string sName = XmlHelper.GetNameAttribute(elEntityType);
                if (String.IsNullOrWhiteSpace(sName)) continue;

                XElement elEntityTypeUsed;
                if (!_mEntityTypeElements.TryGetValue(sName, out elEntityTypeUsed))
                {
                    if (elEntityType.Descendants().Where(el => el.Value == "ModelLink").Count() == 0)
                    {
                        _mEntityTypeElements.Add(sName, elEntityType);
                        _mSchemaElement.Add(elEntityType);
                        elEntityTypeUsed = elEntityType;
                    }
                }

                Dictionary<string, XElement> dictNav;
                if (!_mNavigationProperties.TryGetValue(sName, out dictNav))
                {
                    dictNav = new Dictionary<string, XElement>();
                    _mNavigationProperties.Add(sName, dictNav);
                }

                var elNavigations = elEntityType.Elements().Where(elNav => elNav.Name.LocalName == "NavigationProperty");
                foreach (var elNav in elNavigations)
                {
                    var sNavName = XmlHelper.GetNameAttribute(elNav);
                    
                    if (String.IsNullOrWhiteSpace(sNavName)) continue;
                    if (dictNav.ContainsKey(sNavName)) continue;

                    dictNav.Add(sNavName, elNav);
                    if (elEntityTypeUsed != null)
                        elEntityTypeUsed.Add(elNav);
                    else
                        SaveNavigationPropertiesUntilEntityHasBeenAdded(sNavName, elNav, navigationProperties);
                }
                AddBackAnySavedNavigationProperties(sName, navigationProperties, elEntityTypeUsed,dictNav);
            }

            var elAssociations = lSchema.Elements().Where(el => el.Name.LocalName == "Association");
            foreach (var elAssoc in elAssociations)
            {
                string sName = XmlHelper.GetNameAttribute(elAssoc);
                if (String.IsNullOrWhiteSpace(sName)) continue;

                if (!_mAssociationElements.ContainsKey(sName))
                {
                    _mAssociationElements.Add(sName, elAssoc);
                    _mSchemaElement.Add(elAssoc);
                }
            }

            var elFunctions = lSchema.Elements().Where(el => el.Name.LocalName == "Function");
            foreach (var elFunction in elFunctions)
            {
                string sName = XmlHelper.GetNameAttribute(elFunction);
                if (String.IsNullOrWhiteSpace(sName)) continue;

                if (!_mFunctionElements.ContainsKey(sName))
                {
                    _mFunctionElements.Add(sName, elFunction);
                    _mSchemaElement.Add(elFunction);
                }
            }

            var elComplexTypes = lSchema.Elements().Where(el => el.Name.LocalName == "ComplexType");
            foreach (var elComplexType in elComplexTypes)
            {
                string sName = XmlHelper.GetNameAttribute(elComplexType);
                if (String.IsNullOrWhiteSpace(sName)) continue;

                if (!_mComplexTypeElements.ContainsKey(sName))
                {
                    _mComplexTypeElements.Add(sName, elComplexType);
                    _mSchemaElement.Add(elComplexType);
                }
            }
        }

        private static void AddBackAnySavedNavigationProperties(string sName, Dictionary<string, List<XElement>> navigationProperties, XElement elEntityTypeUsed, Dictionary<string, XElement> navDictionary)
        {
            if (!navigationProperties.ContainsKey(sName))
                return;
            foreach (var navigationProperty in navigationProperties[sName].Where(navigationProperty => !navDictionary.ContainsKey(sName)))
            {
                elEntityTypeUsed.Add(navigationProperty);
            }
            navigationProperties[sName] = new List<XElement>();
        }


        private static void SaveNavigationPropertiesUntilEntityHasBeenAdded(string sName, XElement elNav, Dictionary<string, List<XElement>> navigationProperties)
        {
            if (! navigationProperties.ContainsKey(sName))
                navigationProperties.Add(sName, new List<XElement>());
            
            navigationProperties[sName].Add(elNav);
        }

        private XElement _mSchemaElement;
        private XElement _mEntityContainerElement;
        private readonly Dictionary<string, XElement> _mInContainerSetElements = new Dictionary<string, XElement>();
        private readonly Dictionary<string, XElement> _mEntityTypeElements = new Dictionary<string, XElement>();
        private readonly Dictionary<string, XElement> _mAssociationElements = new Dictionary<string, XElement>();
        private readonly Dictionary<string, XElement> _mFunctionElements = new Dictionary<string, XElement>();
        private readonly Dictionary<string, XElement> _mComplexTypeElements = new Dictionary<string, XElement>();
        private readonly Dictionary<string, Dictionary<string, XElement>> _mNavigationProperties = new Dictionary<string, Dictionary<string, XElement>>();

        private void Initialize()
        {
            _mSchemaElement = (from el in Document.Elements()
                               where el.Name.LocalName == "Schema"
                               select el).FirstOrDefault();
            if (_mSchemaElement == null)
                throw new InvalidOperationException("Invalid sdl data");

            SchemaNamespace = XmlHelper.GetNameAttribute(_mSchemaElement, "Namespace");
            if (String.IsNullOrWhiteSpace(SchemaNamespace))
                throw new InvalidOperationException("Invalid sdl data");
            SchemaNamespace += ".";

            _mEntityContainerElement = (from el in _mSchemaElement.Elements()
                                        where el.Name.LocalName == "EntityContainer"
                                        select el).FirstOrDefault();
            if (_mEntityContainerElement == null)
            {
                _mEntityContainerElement = new XElement("EntityContainer");
                _mSchemaElement.Add(_mEntityContainerElement);
                return;
            }

            var elContainer = _mEntityContainerElement.Elements();
            foreach (var elItemContainer in elContainer)
            {
                string sName = XmlHelper.GetNameAttribute(elItemContainer);
                _mInContainerSetElements.Add(sName, elItemContainer);
            }

            var elEntityTypes = (from el in _mSchemaElement.Elements()
                                 where el.Name.LocalName == "EntityType"
                                 select el);
            foreach (var elEntityType in elEntityTypes)
            {
                string sName = XmlHelper.GetNameAttribute(elEntityType);
                if (String.IsNullOrWhiteSpace(sName)) continue;

                _mEntityTypeElements.Add(sName, elEntityType);

                var dictNav = new Dictionary<string, XElement>();
                _mNavigationProperties.Add(sName, dictNav);

                var elNavigations = (from elItem in elEntityType.Elements()
                                     where elItem.Name.LocalName == "NavigationProperty"
                                     select elItem);
                foreach (var elNav in elNavigations)
                {
                    sName = XmlHelper.GetNameAttribute(elNav);
                    if (String.IsNullOrWhiteSpace(sName)) continue;

                    dictNav.Add(sName, elNav);
                }
            }

            var elAssocs = (from el in _mSchemaElement.Elements()
                            where el.Name.LocalName == "Association"
                            select el);
            foreach (var elAssoc in elAssocs)
            {
                string sName = XmlHelper.GetNameAttribute(elAssoc);
                if (String.IsNullOrWhiteSpace(sName)) continue;

                _mAssociationElements.Add(sName, elAssoc);
            }

            var elFunctions = (from el in _mSchemaElement.Elements()
                            where el.Name.LocalName == "Function"
                            select el);
            foreach (var elAssoc in elFunctions)
            {
                string sName = XmlHelper.GetNameAttribute(elAssoc);
                if (String.IsNullOrWhiteSpace(sName)) continue;

                _mFunctionElements.Add(sName, elAssoc);
            }

            var elComplexTypes = (from el in _mSchemaElement.Elements()
                                  where el.Name.LocalName == "ComplexType"
                            select el);
            foreach (var elComplexType in elComplexTypes)
            {
                string sName = XmlHelper.GetNameAttribute(elComplexType);
                if (String.IsNullOrWhiteSpace(sName)) continue;

                _mComplexTypeElements.Add(sName, elComplexType);
            }

        }
    }
}