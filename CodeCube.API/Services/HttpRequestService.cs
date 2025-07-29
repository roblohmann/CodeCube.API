using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeCube.API.Services;

public class HttpRequestService : IHttpRequestService
{
    private readonly HttpClient _httpClient;

    public HttpRequestService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// This method is used runtime to retrieve an bearer token for authentication.
    /// By default null is return, but this method can be overriden with your own implementation to return an token if
    /// bearer-authentication is required.
    /// </summary>
    /// <returns>An bearertoken.</returns>
    public virtual string? GetBearerToken()
    {
        return null;
    }
    
    public async Task<HttpResponseMessage> HttpGet(string url, CancellationToken cancellationToken)
    {
        return await ExecuteRequest(HttpMethod.Get, null, url, cancellationToken);
    }

    public async Task<HttpResponseMessage> HttpGetWithBody(string url, object body, CancellationToken cancellationToken)
    {
        return await ExecuteRequest(HttpMethod.Get, body, url, cancellationToken);
    }

    public async Task<HttpResponseMessage> HttpPost(string url, object body, CancellationToken cancellationToken)
    {
        return await ExecuteRequest(HttpMethod.Post, body, url, cancellationToken);
    }

    public async Task<HttpResponseMessage> HttpPut(string url, object body, CancellationToken cancellationToken)
    {
        return await ExecuteRequest(HttpMethod.Put, body, url, cancellationToken);
    }

    public async Task<HttpResponseMessage> HttpPatch(string url, object body, CancellationToken cancellationToken)
    {
        return await ExecuteRequest(HttpMethod.Patch, body, url, cancellationToken);
    }

    public async Task<HttpResponseMessage> HttpDelete(string url, object body, CancellationToken cancellationToken)
    {
        return await ExecuteRequest(HttpMethod.Delete, body, url, cancellationToken);
    }
    
    private async Task<HttpResponseMessage> ExecuteRequest(HttpMethod httpMethod, 
        object body, 
        string url, 
        CancellationToken cancellationToken)
    {
        var requestMessage = new HttpRequestMessage(httpMethod, url);
        
        var bearerToken = GetBearerToken();
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);    
        }

        var json = body.SerializeWithCamelCase();

        requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

        return await _httpClient.SendAsync(requestMessage, cancellationToken);
    }
    
    // private async Task<OperationResult<T>> ExecuteRequest<T>(HttpRequestMessage requestMessage, object body, 
    //     string url, 
    //     CancellationToken cancellationToken) where T : new()
    // {
    //     var result = new OperationResult<T>();
    //     requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());
    //
    //     var json = body.DeserializeFromCamelCase<T>();
    //
    //     requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
    //
    //     var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
    //
    //     var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
    //
    //     if (!response.IsSuccessStatusCode)
    //     {
    //         result.OperationInfo = new OperationInfo
    //         {
    //             Message = responseBody,
    //             Url = url,
    //             Status = (int)response.StatusCode,
    //         };
    //
    //         return result;
    //     }
    //
    //     var responseObject = responseBody.SerializeWithCamelCase();
    //
    //     result.OperationInfo = new OperationInfo { Message = null, Url = url, Status = (int)response.StatusCode };
    //     result.Body = responseObject;
    //
    //     return result;
    // }
}