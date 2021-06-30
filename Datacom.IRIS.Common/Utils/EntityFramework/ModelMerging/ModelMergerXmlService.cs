using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Datacom.IRIS.Common.Utils.EntityFramework.ModelMerging
{
    public interface IModelMergerXmlService
    {
        ModelXmlServiceEntityFrameworkv4R0 LoadSchema(FileInfo modelFileInfo);
        void SaveSchema(XDocument document, string path, SaveOptions saveOptions);
        XDocument GetModelSkeletonAsDocument();
    }

    public class ModelMergerXmlService : IModelMergerXmlService
    {
        public ModelXmlServiceEntityFrameworkv4R0 LoadSchema(FileInfo modelFileInfo)
        {
            var stream = new StreamReader(modelFileInfo.FullName).BaseStream;
            if (stream.Length == 0) return null;
            var schemaXDocument = XDocument.Load(stream);
            return new ModelXmlServiceEntityFrameworkv4R0(schemaXDocument);
        }

        public void SaveSchema(XDocument document, string path, SaveOptions saveOptions)
        {
            if (document != null)
                document.Save(path, saveOptions);
        }

        public XDocument GetModelSkeletonAsDocument()
        {
            const string filePath = "Datacom.IRIS.Common.Utils.EntityFramework.ModelMerging" + ".EDMXSkeleton.xml";
            var fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filePath);
            return XDocument.Load(fileStream);
            //var skeletonXmlBuilder = new StringBuilder();
            //skeletonXmlBuilder.Append(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            //skeletonXmlBuilder.Append(@"<edmx:Edmx Version=""2.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">");
            //skeletonXmlBuilder.Append(@"  <!-- EF Runtime content -->");
            //skeletonXmlBuilder.Append(@"  <edmx:Runtime>");
            //skeletonXmlBuilder.Append(@"    <!-- SSDL content -->");
            //skeletonXmlBuilder.Append(@"    <edmx:StorageModels>");
            //skeletonXmlBuilder.Append(@"    </edmx:StorageModels>");
            //skeletonXmlBuilder.Append(@"    <!-- CSDL content -->");
            //skeletonXmlBuilder.Append(@"    <edmx:ConceptualModels>");
            //skeletonXmlBuilder.Append(@"    </edmx:ConceptualModels>");
            //skeletonXmlBuilder.Append(@"    <!-- C-S mapping content -->");
            //skeletonXmlBuilder.Append(@"    <edmx:Mappings>");
            //skeletonXmlBuilder.Append(@"    </edmx:Mappings>");
            //skeletonXmlBuilder.Append(@"  </edmx:Runtime>");
            //skeletonXmlBuilder.Append(@"</edmx:Edmx>");
            //return XDocument.Load(new StringReader(skeletonXmlBuilder.ToString()));
        }
    }
}