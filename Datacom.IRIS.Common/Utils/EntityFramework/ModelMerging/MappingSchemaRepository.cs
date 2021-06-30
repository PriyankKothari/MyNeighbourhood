using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Datacom.IRIS.Common.Utils.EntityFramework.ModelMerging
{
    /// <summary>Access to entity framework mapping schema</summary>
    public class MappingSchemaRepository
    {
        const string EntityElementLocalName = "EntitySetMapping";

        private readonly XDocument _schemaDocument;

        public MappingSchemaRepository(string schemaPath)
         {
             using (var stream = new StreamReader(schemaPath).BaseStream)
             {
                 _schemaDocument = XDocument.Load(stream);
             }
         }

        /// <summary>Returns mappings for entities from the mapping schema</summary>
        /// <param name="name">entity name</param>
        /// <returns>Mappings if found, otherwise null</returns>
        public IEnumerable<XElement> FindEntityMapsByName(string name)
        {
            
            if (_schemaDocument.Root == null) return null;
            var test4 = _schemaDocument.Descendants().Where(el => el.Name.LocalName == EntityElementLocalName);
            var result = _schemaDocument.Descendants()
                                .Where(el => el.Name.LocalName == EntityElementLocalName)
                                .Where(element => element.FirstAttribute.Value == name).ToList();

            return result.Count == 0 ? null : result;
        }
    }
}