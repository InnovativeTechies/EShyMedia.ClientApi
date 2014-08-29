using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EShyMedia.ClientApi.SimpleRestClient
{
    public interface ISimpleRestClient
    {
        //Properties

        string BaseUrl { get; set; }

        //int Platform { get; set; }

        //Version ClientVersion { get; set; }

        string UserAgent { get; set; }

        string MediaType { get; set; }

        RestParameters DefaultParameters { get; set; }

        //Methods

        Task<TResult> MakeRequestAsync<TResult>(string resource, HttpMethod methodType, string securityMethod, string securityToken, RestParameters parameters, int retries=3, CancellationTokenSource token = null);
    
        Task<Stream> GetStreamAsync(string url, string securityMethod, string securityToken, RestParameters parameters);

        void CancelCurrent();
   
        void Cancel(CancellationTokenSource token);

    }
}