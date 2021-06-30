using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Datacom.IRIS.Common.Helpers
{
    public static class XmlHelper
    {
        public static string GetNameAttribute(XElement el, string attributeName = "Name")
        {
            var attr = el.Attribute(attributeName);
            return attr == null ? null : attr.Value;
        }
        /// Deflates an object into an XML string.
        /// </summary>
        /// <param name="o">The object to serialise</param>
        /// <returns>An XML representation of the object</returns>
        /// <remarks>The object must be tagged with XML Serialisation attributes.</remarks>
        public static string Deflate(object o)
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            return Deflate(o, namespaces);
        }

        /// <summary>
        /// Deflates an object into an XML string using the specified encoding
        /// </summary>
        /// <param name="o">The object to serialise</param>
        /// <param name="encoding">The encoding to use</param>
        /// <returns>An XML representation of the object</returns>
        /// <remarks>The object must be tagged with XML Serialisation attributes.</remarks>
        public static string Deflate(object o, Encoding encoding)
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            return Deflate(o, namespaces, encoding);
        }

        /// <summary>
        /// Deflates an object into an XML string using the given Namespaces.
        /// </summary>
        /// <param name="o">The object to serialise</param>
        /// <param name="namespaces">The namespaces to generate the XML into.</param>
        /// <returns>An XML representation of the object</returns>.
        /// <remarks>The object must be tagged with XML Serialisation attributes.</remarks>
        public static string Deflate(object o, XmlSerializerNamespaces namespaces)
        {
            var sb = new StringBuilder();
            var serializer = new XmlSerializer(o.GetType());
            var writer = new StringWriter(sb);

            serializer.Serialize(writer, o, namespaces);
            writer.Close();
            return sb.ToString();
        }

        /// <summary>
        /// Deflates an object into an XML string using the given Namespaces and encoding.
        /// </summary>
        /// <param name="o">The object to serialise</param>
        /// <param name="namespaces">The namespaces to generate the XML into.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>An XML representation of the object</returns>.
        /// <remarks>The object must be tagged with XML Serialisation attributes.</remarks>
        public static string Deflate(object o, XmlSerializerNamespaces namespaces, Encoding encoding)
        {
            using (var ms = new MemoryStream())
            {
                var serializer = new XmlSerializer(o.GetType());
                using (var writer = new XmlTextWriter(ms, encoding))
                {
                    serializer.Serialize(writer, o, namespaces);
                    writer.Close();
                    return encoding.GetString(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Inflates a deflated object back into an object of the given type.
        /// </summary>
        /// <param name="xml">The XML to inflate.</param>
        /// <param name="type">The type to inflate.</param>
        /// <returns>The inflated object</returns>
        public static T Inflate<T>(string xml) where T : class
        {
            var ser = new XmlSerializer(typeof(T));
            using (var sr = new StringReader(xml))
            {
                using (var reader = new XmlTextReader(sr))
                {
                    object cfg = ser.Deserialize(reader);
                    reader.Close();

                    return cfg as T;
                }
            }
        }

        /// <summary>
        /// Inflates a deflated object back into an object of the given type.
        /// </summary>
        /// <param name="xml">The XML to inflate.</param>
        /// <param name="type">The type to inflate.</param>
        /// <returns>The inflated object</returns>
        public static object Inflate(string xml, Type T)
        {
            var ser = new XmlSerializer(T);
            using (var sr = new StringReader(xml))
            {
                using (var reader = new XmlTextReader(sr))
                {
                    object cfg = ser.Deserialize(reader);
                    reader.Close();

                    return cfg;
                }
            }
        }

        /// <summary>
        /// Inflates a deflated object back into an object of the given type, using the spcified encoding.
        /// </summary>
        /// <param name="xml">The XML to inflate.</param>
        /// <param name="type">The type to inflate.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>The inflated object</returns>
        public static T Inflate<T>(string xml, Encoding encoding) where T : class
        {
            byte[] objectInfo = encoding.GetBytes(xml);
            using (var ms = new MemoryStream(objectInfo))
            {
                var ser = new XmlSerializer(typeof(T));
                using (var reader = new XmlTextReader(ms))
                {
                    object cfg = ser.Deserialize(reader);
                    reader.Close();

                    return cfg as T;
                }
            }
        }
    }
}
