//This class is copied from Datacom.DCC.Interaction code base
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;

namespace Datacom.IRIS.Common.Helpers.Rest
{
    public class RestClientException : ApplicationException
    {
        public readonly string RemoteUrl, Response;
        public readonly HttpStatusCode HttpResponseCode;


        public RestClientException(string remoteUrl, string response, HttpStatusCode httpResponseCode, Exception innerException)
            : base(GenerateGeneralExMessage(remoteUrl), innerException)
        {
            RemoteUrl = remoteUrl;
            Response = response;
            HttpResponseCode = httpResponseCode;
        }


        public RestClientException(string remoteUrl, string response, HttpStatusCode httpResponseCode, JsonReaderException innerException)
            : base(GenerateJsonReaderExMessage(remoteUrl, response), innerException)
        {
            RemoteUrl = remoteUrl;
            Response = response;
            HttpResponseCode = httpResponseCode;
        }


        static string GenerateGeneralExMessage(string serverAddress)
        {
            // TODO: Include more info about the request
            return "Exception while talking to " + serverAddress;
        }


        static string GenerateJsonReaderExMessage(string serverAddress, string response)
        {
            return String.Format("Error reading or deserialising response from {0}. Received this response: {1}", serverAddress, response ?? "");
        }
    }
}
