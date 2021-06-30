using System.IO;

namespace Datacom.IRIS.Common.Utils
{
    public class PathBuilder
    {
        private readonly  string _basePath;
        public PathBuilder(string basePath) { _basePath = basePath; }

        public string MOJExportSchema(string xsdFileName)
        {
            return Path.Combine(_basePath, "Validators\\Schemas", xsdFileName);
        }
    }
}