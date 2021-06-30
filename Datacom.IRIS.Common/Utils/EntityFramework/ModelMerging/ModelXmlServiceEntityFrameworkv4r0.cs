using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Datacom.IRIS.Common.Utils.EntityFramework.ModelMerging
{
    /// <summary>Extracts the three schemas from an v4.0 Entity Framework model(.edmx file)</summary>
    public class ModelXmlServiceEntityFrameworkv4R0
    {
        //Some code from http://multifileedmx.codeplex.com and some from http://www.hendryten.com/tag/edmx/ thx.
        //For background information on the purpose of this see http://simonburgoyne.com/blog/?p=347

        /// <summary>EDMX name spaces</summary>
        private readonly static XNamespace Edmxns = "http://schemas.microsoft.com/ado/2009/11/edmx";
        private readonly static XNamespace Edmns = "http://schemas.microsoft.com/ado/2009/11/edm";
        private readonly static XNamespace Ssdlns = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
        private readonly static XNamespace Csns = "http://schemas.microsoft.com/ado/2009/11/mapping/cs";
        public XDocument ModelDocument;

        private const string StorageModelNodeIndex = "StorageModels";
        private const string ConceptualModelNodeIndex = "ConceptualModels";
        private const string MappingsNodeIndex = "Mappings";

        const string SchemaElementIndex = "Schema";

        public bool HasSchema { get { return HasConceptualSchema && HasStorageSchema && HasMappingSchema; } }
        public bool HasConceptualSchema { get { return ConceptualSchema != null; } }
        public bool HasStorageSchema { get { return StorageSchema != null; } }
        public bool HasMappingSchema { get { return MappingSchema != null; } }

        private XElement RuntimeNode { get { return EDMXNode.Element(Edmxns + "Runtime"); } }
        private XElement EDMXNode { get { return ModelDocument.Element(Edmxns + "Edmx"); }}

        /// <summary>Conceptual schema. It describes the entities, relationships, and functions that make up a conceptual model of a data-driven application.</summary>        
        public XElement ConceptualSchema
        {
            get
            {
                var conceptualNode = RuntimeNode.Element(Edmxns + ConceptualModelNodeIndex);
                return conceptualNode != null ? conceptualNode.Element(Edmns + SchemaElementIndex) : null;
            }
            set
            {                
                var conceptualRootElement = RuntimeNode.Element(Edmxns + ConceptualModelNodeIndex);
                if (conceptualRootElement == null) return;

                var schemaElement = conceptualRootElement.Element(Edmns + SchemaElementIndex);
                if (schemaElement == null)
                    conceptualRootElement.Add(value);
                else
                    schemaElement.Value = value.ToString();
            }
        }
        /// <summary>Storage(generally database) schema. It describes the storage model of an Entity Framework application.</summary>
        public XElement StorageSchema
        {
            get
            {
                var storageNode = RuntimeNode.Element(Edmxns + StorageModelNodeIndex);
                return storageNode != null ? storageNode.Element(Ssdlns + SchemaElementIndex) : null;
            }
            set
            {
                var storageRootElement = RuntimeNode.Element(Edmxns + StorageModelNodeIndex);
                if (storageRootElement == null) return;

                var schemaElement = storageRootElement.Element(Ssdlns + SchemaElementIndex);
                if (schemaElement == null)
                    storageRootElement.Add(value);
                else
                    schemaElement.Value = value.ToString();
            }
        }
        /// <summary>Mapping schema. It describes the mapping between the conceptual model and storage model of an Entity Framework application.</summary>
        public XElement MappingSchema
        {
            get
            {
                var mappingNode = RuntimeNode.Element(Edmxns + MappingsNodeIndex);
                return mappingNode != null ? mappingNode.Element(Csns + "Mapping") : null;
            }
            set
            {
                var mappingRootElement = RuntimeNode.Element(Edmxns + MappingsNodeIndex);
                if (mappingRootElement == null) return;

                var schemaElement = mappingRootElement.Element(Csns + "Mapping");
                if (schemaElement == null)
                    mappingRootElement.Add(value);
                else
                    schemaElement.Value = value.ToString();
            }
        }

        /// <summary>Loads the model from a file path</summary>
        /// <param name="modelPath">file path</param>
        public ModelXmlServiceEntityFrameworkv4R0(string modelPath)
        {
            var stream = new StreamReader(modelPath).BaseStream;
            ModelDocument = XDocument.Load(stream);
        }

        /// <summary>Loads the model</summary>
        /// <param name="model">Xml document</param>
        public ModelXmlServiceEntityFrameworkv4R0(XDocument model)
        {
            ModelDocument = model;
        }
    }
}