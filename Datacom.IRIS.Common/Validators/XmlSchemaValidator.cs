using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Datacom.IRIS.Common.Extensions;

namespace Datacom.IRIS.Common.Validators
{
    //Validate XML against schema, raise error if invalid.
    public class XmlSchemaValidator
    {
        private readonly string _xsdPath;
        private readonly XmlDocument _xmldoc ;

        public string ErrorMsg { get; set; }

        public XmlSchemaValidator(string xsdPath, string xmlPath)
        {
            _xsdPath = xsdPath;
            _xmldoc = new XmlDocument();
            _xmldoc.Load(xmlPath);
        }

        public XmlSchemaValidator(string xsdPath, XmlDocument xmldoc)
        {
            _xsdPath = xsdPath;
            _xmldoc = xmldoc;
        }

        public XmlSchemaValidator(string xsdPath, XDocument xmldoc)
        {
            _xsdPath = xsdPath;
            _xmldoc = xmldoc.ToXmlDocument();
        }

        public bool IsValid
        {
            get
            {
                var settings = new XmlReaderSettings();
                settings.Schemas.Add(null, _xsdPath);
                settings.ValidationType = ValidationType.Schema;

                CheckXmlDocImplementsSchema(settings);
                ValidateScheme(settings);             
   
                return true;
            }
        }

        private void ValidateScheme(XmlReaderSettings settings)
        {
            using (var rdr = XmlReader.Create(new StringReader(_xmldoc.InnerXml), settings))
            {
                while (rdr.Read())
                {
                }
            }
        }

        private void CheckXmlDocImplementsSchema(XmlReaderSettings settings)
        {
            var hasSchema = false;
            var xmlSchema = settings.Schemas.Schemas().OfType<XmlSchema>().First();
            
            var xsdNamespace = xmlSchema.TargetNamespace;
            if (_xmldoc.DocumentElement != null)
            {
                var xmlNamespace = _xmldoc.DocumentElement.NamespaceURI;
                if (xsdNamespace == xmlNamespace)
                hasSchema = true;
            }

            if (hasSchema) return;

            var errMsg = string.Format("The xml file does not implement the schema '{0}'.", xsdNamespace);
            throw new XmlSchemaValidationException(errMsg);
        }
    }
}