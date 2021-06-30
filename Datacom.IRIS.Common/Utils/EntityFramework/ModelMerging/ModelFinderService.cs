using System.Collections.Generic;
using System.IO;

namespace Datacom.IRIS.Common.Utils.EntityFramework.ModelMerging
{
    /// <summary>Finds models to merge</summary>
    public static class ModelFinderService
    {
        //For background information on the purpose of this see http://simonburgoyne.com/blog/?p=347

        const string ModelSearchPattern = "*.edmx";

        /// <summary>Finds models given a list of file paths</summary>
        /// <param name="paths"></param>
        /// <param name="modelToIgnore">Can ignore a model this is used to ignore merged model.</param>
        /// <returns></returns>
        public static IEnumerable<FileInfo> Find(IEnumerable<string> paths, string modelToIgnore = null)
        {
            var result = new List<FileInfo>();
            paths.ForEach(path =>
                              {
                                  DirectoryInfo dirInfo;
                                  if ((dirInfo = new DirectoryInfo(path)).Exists)
                                  {
                                      result.AddRange(dirInfo.GetFiles(ModelSearchPattern));
                                  }
                              });
            if (modelToIgnore != null) result.RemoveAll(x => x.Name == modelToIgnore);
            return result;
        }
    }
}