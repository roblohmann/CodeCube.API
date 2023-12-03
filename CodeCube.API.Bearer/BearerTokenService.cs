using System.Net;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace CodeCube.API.Bearer
{
    public sealed class BearerTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;

        public BearerTokenService(HttpClient httpClient, IConfiguration configuration, ILogger<BearerTokenService> logger, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="uniqueIdentifier"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public async Task<string?> RequestToken(Guid uniqueIdentifier, string scope)
        {
            var cacheKey = $"{uniqueIdentifier}-token";

            var token = _memoryCache.Get<string>(cacheKey);
            if (token != null)
                return token;

            var identitySection = _configuration.GetSection("Identity");
            var clientIdPrefix = identitySection["ClientIdPrefix"];

            var url = identitySection["TokenUrl"];
            var clientId = $"{clientIdPrefix}.{uniqueIdentifier}".ToLower();
            var clientSecret = identitySection["ClientSecret"];

            var tokenRequest = new TokenRequest
            {
                Address = url,
                ClientId = clientId,
                ClientSecret = clientSecret,
                GrantType = IdentityModel.OidcConstants.GrantTypes.ClientCredentials,
                Parameters =
                {
                    {"scope", scope}
                }
            };

            var response = await _httpClient.RequestTokenAsync(tokenRequest);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                token = response.AccessToken;

                _memoryCache.Set(cacheKey, token, TimeSpan.FromSeconds(response.ExpiresIn * 0.65));

                return token;
            }

            var errorMessage = $"Unable to retrieve token for client {clientId} from provider '{url}'. Error: '{await response.HttpResponse.Content.ReadAsStringAsync()}'";
            if (response.Exception != null)
            {
                errorMessage = $"{errorMessage}, exception {response.Exception.GetBaseException().Message}";
            }

            _logger.LogError(errorMessage);

            return null;
        }
    }
}
