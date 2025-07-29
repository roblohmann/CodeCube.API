using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CodeCube.API.Services;

public interface IHttpRequestService
{
    string? GetBearerToken();

    Task<HttpResponseMessage> HttpGet(string url, CancellationToken cancellationToken);
    Task<HttpResponseMessage> HttpGetWithBody(string url, object body, CancellationToken cancellationToken);

    Task<HttpResponseMessage> HttpPost(string url, object body, CancellationToken cancellationToken);

    Task<HttpResponseMessage> HttpPut(string url, object body, CancellationToken cancellationToken);
    
    Task<HttpResponseMessage> HttpPatch(string url, object body, CancellationToken cancellationToken);
    
    Task<HttpResponseMessage> HttpDelete(string url, object body, CancellationToken cancellationToken);
}