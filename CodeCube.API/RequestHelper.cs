using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace CodeCube.API
{
    public static class RequestHelper
    {
        /// <summary>
        /// Get a <see cref="StringContent"/> object to use with an API-request. 
        /// </summary>
        /// <param name="requestObject">The object to serialize to <see cref="StringContent"/></param>
        /// <returns>Stringcontent object with UTF-8 encoding and application/json as mediatype.</returns>
        public static StringContent GetRequestContent(object requestObject)
        {
            return new StringContent(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json");
        }
    }
}
