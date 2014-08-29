#Simple Rest Client for MvvmCross

##Features
* No more duplicating of HttpClient code for every Api call you make.
* Uri is built from the parameters including url segments and query string (based on RestSharp.org's implementation)
* Resource syntax is the same as Asp.Net Web Api Routing Attribute (if you use these attributes you can just copy the resource over)
* Deserializes the result (using Json.NET)
* Uses ModernHttpClient (using Cheesebaron's MvvmCross plugin) for great performance on Android and iOS

##Getting Started

###Add Libraries

Add EShyMedia.ClientApi.SimpleRestClient to your Core project (or wherever you intend to make the Api calls)
 
```
Install-Package EShyMedia.ClientApi.SimpleRestClient
```

In your UI projects, add the [ModernHttpClient plugin](https://github.com/Cheesebaron/Cheesebaron.MvxPlugins)
```
Install-Package EShyMedia.MvvmCross.Plugins.ModernHttpClient
```

###Create a service
Create the interface and class for a service

Here's a sample service:
```
public interface ISampleService {
    Task<List<Product>> GetProductsAsync(int pageNumber, int pageSize);
    Task<Product> GetProductDetails(int productId);
    Task<Product> AddNewProduct(Product product);
}

public class SampleService:ISampleService
{
    private readonly ISimpleRestClient _simpleRestClient;
    public SampleService (ISimpleRestClient simpleRestClient) 
    {
        _simpleRestClient = simpleRestClient;
        _simpleRestClient.BaseUrl = "http://yourapibaseurl.com/";
    }

    public async Task<List<Product>> GetProductsAsync(int pageNumber, int pageSize) 
    {
        //The RestParameters will contain Url segments, querystring parameters, headers and the body contents for POST/PUT calls
        var parameters = new RestParameters();
        parameters.AddParameter("page", pageNumber);
        parameters.AddParameter("size", pageSize);

        //Make the request (this request will be a GET on http://yourapibaseurl.com/Products?page=n1&size=n2)
        //Use SecurityMethod, SecurityToken to add an Authorization header (you can also add an authorization header to the Parameters collection)
        //Retries defines the number of attempts in case a call fails before giving up
        var result = await _simpleRestClient.MakeRequestAsync<List<Product>>("Products", HttpMethod.Get, SecurityMethod, SecurityToken, parameters, Retries);

        return result;
    }

    public async Task<Product> GetProductDetails(int productId) 
    {
        var parameters = new RestParameters();
        parameters.AddUrlSegment("productId", productId);

        var result = await _simpleRestClient.MakeRequestAsync<Product>("Products/{productId}", HttpMethod.Get,
                    SecurityMethod, SecurityToken, parameters);

        return result;
    }

    public async Task<Product> AddNewProduct(Product product) 
    {
        var parameters = new RestParameters();
        parameters.AddParameter("body", product, RestParameterTypes.Body);

        var result =
            await
                _simpleRestClient.MakeRequestAsync<Product>("Products", HttpMethod.Post, SecurityMethod, SecurityToken, parameters, 1);

        return result;    
    }

}

```