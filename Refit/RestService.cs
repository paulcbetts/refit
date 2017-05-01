using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

namespace Refit
{
    interface IRestService
    {
        T For<T>(HttpClient client);
    }

    public static class RestService
    {
        public static T For<T>(HttpClient client, RefitSettings settings)
        {
            var className = "AutoGenerated" + typeof(T).Name;
            var requestBuilder = RequestBuilder.ForType<T>(settings);
            var typeName = typeof(T).AssemblyQualifiedName.Replace(typeof(T).Name, className);
            var generatedType = Type.GetType(typeName);

            if(generatedType == null) { 
                var message = typeof(T).Name + " doesn't look like a Refit interface. Make sure it has at least one " + 
                    "method with a Refit HTTP method attribute and Refit is installed in the project.";

                throw new InvalidOperationException(message);
            }

            return (T)Activator.CreateInstance(generatedType, client, requestBuilder);
        }

        public static T For<T>(HttpClient client)
        {
            return RestService.For<T>(client, null);
        }

        public static T For<T>(string hostUrl, RefitSettings settings)
        {
#if PORTABLE
            throw new NotImplementedException("You've somehow included the PCL version of Refit in your app. You need to use the platform-specific version!");
#else
            // check to see if user provided custom auth t

            HttpMessageHandler innerHandler = null;
            if (settings != null) {
                if (settings.HttpMessageHandlerFactory != null) {
                    innerHandler = settings.HttpMessageHandlerFactory();
                }

                if (settings.AuthorizationHeaderValueGetter != null) {
                    innerHandler = new AuthenticatedHttpClientHandler(settings.AuthorizationHeaderValueGetter, settings.AuthorizationHeaderRefreshedValueGetter, innerHandler);
                }
            }

            var client = new HttpClient(innerHandler ?? new HttpClientHandler()) { BaseAddress = new Uri(hostUrl) };
            return RestService.For<T>(client, settings);
#endif

        }

        public static T For<T>(string hostUrl)
        {
            return RestService.For<T>(hostUrl, null);
        }
    }

    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }
        public HttpResponseHeaders Headers { get; private set; }
        public HttpMethod HttpMethod { get; private set; }
        public Uri Uri { get; private set; }

        public HttpContentHeaders ContentHeaders { get; private set; }

        public string Content { get; private set; }

        public bool HasContent
        {
            get { return !String.IsNullOrWhiteSpace(Content); }
        }
        public RefitSettings RefitSettings { get; set; }

        ApiException(Uri uri, HttpMethod httpMethod, HttpStatusCode statusCode, string reasonPhrase, HttpResponseHeaders headers, RefitSettings refitSettings = null) :
            base(createMessage(statusCode, reasonPhrase))
        {
            Uri = uri;
            HttpMethod = httpMethod;
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            Headers = headers;
            RefitSettings = refitSettings;
        }

        public T GetContentAs<T>()
        {
            return HasContent ?
                JsonConvert.DeserializeObject<T>(Content, RefitSettings.JsonSerializerSettings) :
                default(T);
        }

        public static async Task<ApiException> Create(Uri uri, HttpMethod httpMethod, HttpResponseMessage response, RefitSettings refitSettings = null)
        {
            var exception = new ApiException(uri, httpMethod, response.StatusCode, response.ReasonPhrase, response.Headers, refitSettings);

            if (response.Content == null)
                return exception;

            try {
                exception.ContentHeaders = response.Content.Headers;
                exception.Content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                response.Content.Dispose();
            } catch {
                // NB: We're already handling an exception at this point, 
                // so we want to make sure we don't throw another one 
                // that hides the real error.
            }

            return exception;
        }

        static string createMessage(HttpStatusCode statusCode, string reasonPhrase)
        {
            return String.Format("Response status code does not indicate success: {0} ({1}).", (int)statusCode, reasonPhrase);
        }
    }
}
