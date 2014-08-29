using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cheesebaron.MvxPlugins.ModernHttpClient;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;

namespace EShyMedia.ClientApi.SimpleRestClient
{
    public class SimpleRestClient : ISimpleRestClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMvxJsonConverter _jsonConverter;
        private CancellationTokenSource _currentToken;

        public SimpleRestClient(IHttpClientFactory httpClientFactory, IMvxJsonConverter jsonConverter)
        {
            _httpClientFactory = httpClientFactory;
            _jsonConverter = jsonConverter;
            if (String.IsNullOrWhiteSpace(MediaType))
                MediaType = "application/json";
            if (DefaultParameters == null)
            {
                DefaultParameters = new RestParameters();
            }
        }

        #region Properties

        public string BaseUrl { get; set; }
        //public int Platform { get; set; }
        //public Version ClientVersion { get; set; }
        public string UserAgent { get; set; }
        public string MediaType { get; set; }
        public RestParameters DefaultParameters { get; set; }

        #endregion

        #region public methods

        public async Task<TResult> MakeRequestAsync<TResult>(string resource, HttpMethod methodType,
            string securityMethod,string securityToken, 
            RestParameters parameters, int retries = 3, CancellationTokenSource token = null)
        {
            Mvx.TaggedTrace("RestClient", "MakeRequestAsync - Start - {0})", resource);

            _currentToken = token ?? new CancellationTokenSource();

            var handler = _httpClientFactory.GetHandler();
            var outerHandler = new RetryHandler(handler, retries);
            var client = _httpClientFactory.Get(outerHandler);

            var allParams = new RestParameters();
            allParams.AddRange(DefaultParameters);
            allParams.AddRange(parameters);

            // Build the uri
            var relativeAddress = BuildUri(BaseUrl, resource, allParams);

            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType));

            if (!String.IsNullOrWhiteSpace(securityToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(String.IsNullOrWhiteSpace(securityMethod) ? "OAuth" : securityMethod, securityToken);
            }

            var request = new HttpRequestMessage(methodType, relativeAddress);

            //Add Body
            var bodyParam = allParams.FirstOrDefault(p => p.ParameterType == RestParameterTypes.Body);
            if (bodyParam != null)
            {
                if (bodyParam.Value.GetType() == typeof(MemoryStream))
                {
                    request.Content = new StreamContent(bodyParam.Value as MemoryStream);
                }
                else
                {
                    var serialized = Serialize(bodyParam.Value);
                    request.Content = new StringContent(serialized, Encoding.UTF8, MediaType);
                }
            }

            //Add headers
            foreach (var p in allParams.Where(p => p.ParameterType == RestParameterTypes.Header))
            {
                request.Headers.Add(p.Name, p.Value.ToString());
            }
            
            //request.Headers.Add("Platform", Platform.ToString());

            //request.Headers.Add("ClientVersion", ClientVersion.ToString());

            //Make Request
            Mvx.TaggedTrace("RestClient", "MakeRequestAsync - Request - {0}", request.RequestUri.ToString());
            var result = await client.SendAsync(request, _currentToken.Token);
            Mvx.TaggedTrace("RestClient", "MakeRequestAsync - Result - {0} - {1}", request.RequestUri.ToString(), result.StatusCode);

            switch (result.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.Unauthorized:
                    result.Dispose();
                    throw new UnauthorizedAccessException(request.RequestUri.ToString());
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.BadRequest:
                    try
                    {
                        //Error message might be in the content, try to get it and pass it up to the caller
                        var content = await result.Content.ReadAsStringAsync();
                        result.Dispose();
                        throw new ProtocolException(content);
                    }
                    catch (Exception ex)
                    {
                        throw new ProtocolException(ex.Message);
                    }
                case HttpStatusCode.NotAcceptable:
                    try
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        result.Dispose();
                        return Deserialize<TResult>(content);
                    }
                    catch (Exception ex)
                    {
                        throw new EndpointNotFoundException(request.RequestUri.ToString());
                    }
                case HttpStatusCode.NotFound:
                    throw new EndpointNotFoundException(request.RequestUri.ToString());
                case HttpStatusCode.NoContent:
                    throw new ProtocolException("NoContent");
                case HttpStatusCode.Accepted:
                case HttpStatusCode.Created:
                case HttpStatusCode.OK:
                    var contents = await result.Content.ReadAsStringAsync();
                    result.Dispose();
                    return Deserialize<TResult>(contents);
                default:
                    result.Dispose();
                    throw new Exception(request.RequestUri.ToString());
            }
        }

        public async Task<Stream> GetStreamAsync(string url, string securityMethod, string securityToken,
            RestParameters parameters)
        {
            var handler = _httpClientFactory.GetHandler();
            var outerHandler = new RetryHandler(handler, 3);
            var client = _httpClientFactory.Get(outerHandler);

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            client.MaxResponseContentBufferSize = 3000000;
            
            try
            {
                var result = await client.GetStreamAsync(url);

                return result;
            }
            catch (EndpointNotFoundException)
            {

            }
            catch (Exception)
            {

            }

            return null;
        }

        public void CancelCurrent()
        {
            if (_currentToken != null)
                _currentToken.Cancel();
        }

        public void Cancel(CancellationTokenSource token)
        {
            if (token == null)
                CancelCurrent();
            else
                token.Cancel();
        }


        #endregion

        #region private methods

        private static string BuildUri(string baseUrl, string resource, List<RestParameter> parameters)
        {
            if (String.IsNullOrWhiteSpace(resource))
                return null;

            var assembled = resource;
            var urlParms = parameters.Where(p => p.ParameterType == RestParameterTypes.UrlSegment);
            assembled = urlParms.Aggregate(assembled, (current, p) => current.Replace("{" + p.Name + "}", p.Value.ToString().UrlEncode()));

            if (!string.IsNullOrEmpty(assembled) && assembled.StartsWith("/"))
            {
                assembled = assembled.Substring(1);
            }

            if (!string.IsNullOrEmpty(baseUrl))
            {
                assembled = string.IsNullOrEmpty(assembled) ? baseUrl : string.Format("{0}/{1}", RemoveTrailingSlash(baseUrl), assembled);
            }

            // build and attach querystring
            if (parameters.Any(p => p.ParameterType == RestParameterTypes.QueryString))
            {
                var data = EncodeParameters(parameters);
                assembled = string.Format("{0}?{1}", RemoveTrailingSlash(assembled), data);
            }

            return assembled;

        }

        private static string RemoveTrailingSlash(string urlpart)
        {
            if (!string.IsNullOrWhiteSpace(urlpart) && urlpart.EndsWith("/"))
            {
                urlpart = urlpart.Substring(0, urlpart.Length - 1);
            }
            return urlpart;
        }

        private static string EncodeParameters(IEnumerable<RestParameter> parameters)
        {
            var querystring = new StringBuilder();
            foreach (var p in parameters.Where(p => p.ParameterType == RestParameterTypes.QueryString))
            {
                if (querystring.Length > 1)
                    querystring.Append("&");
                querystring.AppendFormat("{0}={1}", p.Name.UrlEncode(), (p.Value.ToString()).UrlEncode());
            }

            return querystring.ToString();
        }

        //TODO: Use Media Type to choose serialization method
        private string Serialize(object toSerialize)
        {
            return _jsonConverter.SerializeObject(toSerialize);
        }
        private T Deserialize<T>(string responseBody)
        {
            var toReturn = _jsonConverter.DeserializeObject<T>(responseBody);
            return toReturn;
        }        

        #endregion

    }
}