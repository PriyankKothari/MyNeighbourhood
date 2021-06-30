//This class is copied from Datacom.DCC.Interaction code base
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.Common.Helpers.Rest
{
    /// <summary>
    /// RestClientResponse represents the response from the receiving end of a RestClient call.
    /// It encapsulates the response object which you're after as well as the associated 
    /// HttpWebResponse (which contains the HTTP response code etc.)
    /// </summary>
    /// <typeparam name="T">
    /// The don't care scenario is represented by T being object
    /// </typeparam>
    public class RestClientResponse<T>
    {
        public int HttpResponseCode { get; set; }

        // TODO: Other useful stuff from HttpWebResponse

        public T ResponseObject { get; set; }
    }
}
