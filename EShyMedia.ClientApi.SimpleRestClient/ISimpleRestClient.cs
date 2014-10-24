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

        string UserAgent { get; set; }

        string MediaType { get; set; }

        RestParameters DefaultParameters { get; set; }

        //Methods

        Task<TResult> MakeRequestAsync<TResult>(string resource, HttpMethod methodType, RestParameters parameters, int retries=3, CancellationTokenSource token = null, string mediaType=null);
    
        Task<Stream> GetStreamAsync(string url, RestParameters parameters);

        void CancelCurrent();
   
        void Cancel(CancellationTokenSource token);

    }
}