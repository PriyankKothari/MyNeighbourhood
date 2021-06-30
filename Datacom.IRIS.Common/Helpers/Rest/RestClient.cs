//This class is copied from Datacom.DCC.Interaction code base
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;


namespace Datacom.IRIS.Common.Helpers.Rest
{
    public static class RestClient
    {

        /// <summary>
        /// <para>
        /// Makes a generic call to a web service which accepts and returns JSON. 
        /// </para>
        /// <para>
        /// TODO: Built-in 404 handling?
        /// Things that might need to be made configurable (TODO):
        ///     - HttpWebRequest.SendChunked
        ///     - HttpWebRequest.Accept / HttpWebRequest.ContentType if we decide this should support anything other than JSON
        ///     - HttpWebRequest.ProtocolVersion
        ///     - Proxy stuff
        ///     - Authentication stuff (almost certainly)
        /// </para>
        /// </summary>
        /// <param name="proxy">Can be null</param>
        /// <param name="requestObject">If the specified HTTP method is GET or DELETE, this parameter will be ignored</param>
        /// <typeparamref name="TReturn">Specifying 'object' means that you don't care about the response</typeparamref>
        static RestClientResponse<TReturn> JsonCall<TRequest, TReturn>(HttpMethod.HttpVerbs method, string absoluteUri,
            TRequest requestObject, Action<HttpWebRequest> postInitialisationActions, IWebProxy proxy)
            where TRequest : class
            where TReturn : class
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(absoluteUri);

            webRequest.Method = HttpMethod.ToString(method);
            webRequest.ProtocolVersion = HttpVersion.Version11;
            webRequest.Accept = webRequest.ContentType = "application/json";
            // TODO Might want to remove this
            webRequest.Proxy = proxy;

            if (postInitialisationActions != null)
                postInitialisationActions(webRequest);


            // Only get the request stream for relevant methods
            if (method == HttpMethod.HttpVerbs.Post || method == HttpMethod.HttpVerbs.Put)
            {
                // Send request as JSON, if present
                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    if (requestObject != null)
                    {
                        using (var jsonWriter = new JsonTextWriter(new StreamWriter(requestStream)))
                            new JsonSerializer().Serialize(jsonWriter, requestObject);
                    }
                }
            }


            string jsonResponseString = null;

            // Note that this will only be assigned if the code makes it past GetResponse()
            // This is used by the JsonReaderException catch branch.
            HttpStatusCode returnHttpCode = default(HttpStatusCode);

            try
            {
                // Grab response
                using (var webResponse = webRequest.GetResponse())
                using (var webResponseStream = webResponse.GetResponseStream())
                {
                    returnHttpCode = ((HttpWebResponse)webResponse).StatusCode;

                    // Only deserialise the response if the caller has requested it (object means you don't want to)
                    //Commenting this for now as there is a need for object type to be returned.
                    /*if (typeof(TReturn) == typeof(object))
                    {
                        return new RestClientResponse<TReturn>
                        {
                            HttpResponseCode = (int)((HttpWebResponse)webResponse).StatusCode
                        };
                    }*/

                    using (var responseReader = new StreamReader(webResponseStream, Encoding.UTF8))
                    {
                        // Read into a string before deserialising for debugging only
                        jsonResponseString = responseReader.ReadToEnd();
                        //new EntLibLogger().InfoVariable("jsonResponseString", (jsonResponseString));

                        using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonResponseString)))
                        {
                            return new RestClientResponse<TReturn>
                            {
                                ResponseObject = (TReturn)new JsonSerializer().Deserialize(new StreamReader(memoryStream), typeof(TReturn)),
                                HttpResponseCode = (int)((HttpWebResponse)webResponse).StatusCode
                            };
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                throw new RestClientException(absoluteUri, jsonResponseString, ((HttpWebResponse)ex.Response).StatusCode, ex);
            }
            catch (JsonReaderException ex)
            {
                throw new RestClientException(absoluteUri, jsonResponseString, returnHttpCode, ex);
            }
        }


        public static TReturn JsonGet<TReturn>(string webServiceUri, Action<HttpWebRequest> postInitialisationActions = null)
            where TReturn : class
        {
            return JsonCall<object, TReturn>(HttpMethod.HttpVerbs.Get, webServiceUri, null, postInitialisationActions, null).ResponseObject;
        }


        public static RestClientResponse<TReturn> JsonGetWithResponse<TReturn>(string webServiceUri)
            where TReturn : class
        {
            return JsonCall<object, TReturn>(HttpMethod.HttpVerbs.Get, webServiceUri, null, null, null);
        }


        public static void JsonPost<TRequest>(string webServiceUri, TRequest requestObject)
            where TRequest : class
        {
            JsonCall<TRequest, object>(HttpMethod.HttpVerbs.Post, webServiceUri, requestObject, null, null);
        }


        /// <returns>Currently returns the HTTP response code (this will change!)</returns>
        public static int JsonPostWithResponse<TRequest>(string webServiceUri, TRequest requestObject)
            where TRequest : class
        {
            return JsonCall<TRequest, object>(HttpMethod.HttpVerbs.Post, webServiceUri, requestObject, null, null).HttpResponseCode;
        }

        public static TReturn JsonPost<TRequest, TReturn>(string webServiceUri, TRequest requestObject, Action<HttpWebRequest> postInitialisationActions = null, IWebProxy proxy = null)
            where TRequest : class
            where TReturn : class
        {
            return JsonCall<TRequest, TReturn>(HttpMethod.HttpVerbs.Post, webServiceUri, requestObject, postInitialisationActions, proxy).ResponseObject;
        }


        public static RestClientResponse<TReturn> JsonPostWithResponse<TRequest, TReturn>(string webServiceUri, TRequest requestObject)
            where TRequest : class
            where TReturn : class
        {
            return JsonCall<TRequest, TReturn>(HttpMethod.HttpVerbs.Post, webServiceUri, requestObject, null, null);
        }


        public static TReturn JsonPut<TRequest, TReturn>(string webServiceUri, TRequest requestObject)
            where TRequest : class
            where TReturn : class
        {
            return JsonCall<TRequest, TReturn>(HttpMethod.HttpVerbs.Put, webServiceUri, requestObject, null, null).ResponseObject;
        }


        public static RestClientResponse<TReturn> JsonPutWithResponse<TRequest, TReturn>(string webServiceUri, TRequest requestObject)
            where TRequest : class
            where TReturn : class
        {
            return JsonCall<TRequest, TReturn>(HttpMethod.HttpVerbs.Put, webServiceUri, requestObject, null, null);
        }


        public static void Delete(string webServiceUri)
        {
            JsonCall<object, object>(HttpMethod.HttpVerbs.Delete, webServiceUri, null, null, null);
        }


        /// <param name="webServiceUri"></param>
        /// <returns>Currently returns the HTTP response code (this will change!)</returns>
        public static int DeleteWithResponse(string webServiceUri)
        {
            return JsonCall<object, object>(HttpMethod.HttpVerbs.Delete, webServiceUri, null, null, null).HttpResponseCode;
        }

        public static void Post(string webServiceUri, Dictionary<string, string> paramters)
        {
            var populatedEndPoint = CreateFormattedPostRequest(paramters);
            byte[] bytes = Encoding.UTF8.GetBytes(populatedEndPoint);

            HttpWebRequest request = CreateWebRequest(webServiceUri, bytes.Length);

            try
            {
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                }
            }
            catch (WebException wex)
            {
                throw new RestClientException(webServiceUri, ((HttpWebResponse)wex.Response).StatusDescription, ((HttpWebResponse)wex.Response).StatusCode, wex);
            }
            catch (Exception ex)
            {
                throw new RestClientException(webServiceUri, "Unexpected error", 0, ex);
            }
        }

        private static HttpWebRequest CreateWebRequest(string webServiceUri, Int32 contentLength)
        {
            var request = (HttpWebRequest)WebRequest.Create(webServiceUri);

            request.Method = "POST";
            request.ContentLength = contentLength;
            request.ContentType = "application/x-www-form-urlencoded";

            return request;
        }

        private static string CreateFormattedPostRequest(ICollection<KeyValuePair<string, string>> values)
        {
            var paramterBuilder = new StringBuilder();
            var counter = 0;
            foreach (var value in values)
            {
                paramterBuilder.AppendFormat("{0}={1}", value.Key, HttpUtility.UrlEncode(value.Value));

                if (counter != values.Count - 1)
                {
                    paramterBuilder.Append("&");
                }

                counter++;
            }

            return paramterBuilder.ToString();
        }


    }
}
