//This class is copied from Datacom.DCC.Interaction code base
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.Common.Helpers.Rest
{
    public static class HttpMethod
    {
        /// <summary>
        /// This is a duplication of the HttpVerbs enum in the MVC assembly. This allows us to avoid
        /// taking a dependency on it
        /// </summary>
        public enum HttpVerbs
        {
            Get,
            Post,
            Put,
            Delete,
            Head
        }

        public static string ToString(HttpVerbs httpVerb)
        {
            return httpVerb.ToString().ToUpperInvariant();
        }
    }
}
