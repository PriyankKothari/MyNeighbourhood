using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Datacom.IRIS.Common.Utils.EntityFramework.ModelMerging
{
    /// <summary>Access to entity framework schema that is used for the conceptual and data store models.</summary>
    public class SchemaRepository
    {
        //Element local names
        const string EntityContainerElementLocalName = "EntityContainer";
        const string FunctionElementLocalName = "Function";
        const string ComplexTypeElementLocalName = "ComplexType";
        const string FunctionImportElementLocalName = "FunctionImport";
        const string EntityElementLocalName = "EntityType";
        const string AssociationElementLocalName = "Association";
        const string NavigationPropertyElementLocalName = "NavigationProperty";
        const string LongDescriptionElementLocalName = "LongDescription";

        //Element attribute names
        private const string FromRole = "FromRole";
        private const string ToRole = "ToRole";

        private const string LinkedEntityDescriptionText = "ModelLink";
        
        private readonly XDocument _schemaDocument;

        public SchemaRepository(string schemaPath)
         {
             using (var stream = new StreamReader(schemaPath).BaseStream)
             {
                 _schemaDocument = XDocument.Load(stream);
             }
         }

        /// <summary>Returns entities from the schema</summary>
        /// <param name="name">entity name</param>
        /// <returns>Entities if found, otherwise null</returns>
        public IEnumerable<XElement> FindEntitiesByName(string name)
        {
            return Find(name, EntityElementLocalName);
        }

        public IEnumerable<XElement> FindFunctionImportInEntityContainer(string entityContainerName, string name)
        {
            return FindInEntityContainer(entityContainerName, name, FunctionImportElementLocalName);
        }

        public IEnumerable<XElement> FindFunctionByName(string name)
        {
            return Find(name, FunctionElementLocalName);
        }

        public IEnumerable<XElement> FindComplexTypeByName(string name)
        {
            return Find(name, ComplexTypeElementLocalName);
        }


        public IEnumerable<XElement> FindAssociationByName(string name)
        {
            return Find(name, AssociationElementLocalName);
        }

        public List<XElement> FindInEntityContainer(string entityContainerName, string name, string localName)
        {
            IEnumerable<XElement> elEntityContainer = Find(entityContainerName, EntityContainerElementLocalName);
            var result = elEntityContainer.Elements()
                            .Where(el => el.Name.LocalName == localName)
                            .Where(element => element.FirstAttribute.Value == name).ToList();

            return result.Count == 0 ? null : result;
        }

        /// <summary>Returns navigation properties</summary>
        /// <param name="from">Entity the navigation is from</param>
        /// <param name="to">Entity the navigation is to</param>
        /// <returns>Navigation Properties that match</returns>
        public IEnumerable<XElement> FindNavigationPropertyByNames(string from, string to)
        {
            if (_schemaDocument.Root == null) return null;

            var elements = _schemaDocument.Descendants().Elements().Where(el => el.Name.LocalName == NavigationPropertyElementLocalName);
            var result = elements.Where(element => element.Attributes()
                            .Where(x => x.Name.LocalName == FromRole && x.Value == from).Count() != 0 && element.Attributes()
                            .Where(x => x.Name.LocalName == ToRole && x.Value == to).Count() != 0).ToList();

            return result.Count == 0 ? null : result;
        }

        public IEnumerable<XElement> Find(string name, string localName)
        {
            if (_schemaDocument.Root == null) return null;

            var result = _schemaDocument.Root.Elements()
                                .Where(el => el.Name.LocalName == localName)
                                .Where(element => element.FirstAttribute.Value == name).ToList();

            return result.Count == 0 ? null : result;
        }

        public bool HasLinkedEntities()
        {
            return _schemaDocument.Descendants().Elements().Where(el => el.Value == LinkedEntityDescriptionText).Count() > 0;
        }
    }
}