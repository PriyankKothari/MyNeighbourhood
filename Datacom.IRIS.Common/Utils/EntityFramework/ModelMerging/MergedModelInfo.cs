using System.Collections.Generic;

namespace Datacom.IRIS.Common.Utils.EntityFramework.ModelMerging
{
    /// <summary>Information about models that where merged including the output file paths</summary>
    public class MergedModelInfo
    {
        public MergedModelInfo()
        {
            ValidationWarningMessages = new List<KeyValuePair<string, string>>();
        }

        //For background information on the purpose of this see http://simonburgoyne.com/blog/?p=347

        /// <summary>File path to the merged conceptual schema. It describes the entities, relationships, and functions that make up a conceptual model of a data-driven application.</summary>
        public string ConceptualSchemaFilePath { get; set; }

        /// <summary>File path to the merged data store schema. It describes the storage model of an Entity Framework application. </summary>
        public string DatastoreSchemaFilePath { get; set; }

        /// <summary>File path to the merged mapping schema. It describes the mapping between the conceptual model and storage model of an Entity Framework application.</summary>
        public string MappingSchemaFilePath { get; set; }

        /// <summary>File path to the merged model. This is a rebuild .edmx file without the designer schema. Need for generating self tracking entities.</summary>
        public string ModelFilePath { get; set; }

        /// <summary>Any messages, generally warnings or errors that have occurred during processing.</summary>
        public string Message { get; set; }

        /// <summary>Set to true if the process is successful</summary>
        public bool Result { get; set; }

        /// <summary>
        /// The list of validation warnings of the edmx models found during the validation process.
        /// KeyValuePair<string, string> => KeyValuePair<WarningType, WarningErrorMsg>
        /// </summary>
        public List<KeyValuePair<string, string>> ValidationWarningMessages { get; set; }
    }
}