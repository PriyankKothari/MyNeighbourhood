using System;
using System.Runtime.Serialization;

namespace Datacom.IRIS.Common.Implementations
{
    [Serializable]
    public class SQLResponse
    {
        [DataMember]
        public string Query { get; private set; }

        [DataMember]
        public TimeSpan Duration { get; private set; }

        [DataMember]
        public bool IsSuccessful { get; set; }

        public SQLResponse(string query, TimeSpan duration, bool isSuccessful)
        {
            Query = query;
            Duration = duration;
            IsSuccessful = isSuccessful;
        }
    }
}
