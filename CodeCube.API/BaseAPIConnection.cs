using System;
using System.Collections.Generic;

namespace CodeCube.API
{
    public abstract class BaseAPIConnection
    {
        private readonly int TokenCacheExpirationInMinutes;
        private readonly bool UseLazyMemoryCache;
        private readonly string Scopes;
        private readonly string TokenUrl;
        private readonly string SharedScret;

        private readonly IMemoryCache _memoryCache;

        protected BaseAPIConnection(IMemoryCache memoryCache, string scopes, string tokenUrl, string ssoSharedSecret, int tokenCacheExpirationInMinutes = 50)
        {
            TokenCacheExpirationInMinutes = tokenCacheExpirationInMinutes;
            Scopes = scopes;
            TokenUrl = tokenUrl;
            SharedScret = ssoSharedSecret;

            UseLazyMemoryCache = true;
            _memoryCache = memoryCache;
        }

        protected BaseAPIConnection(string scopes, string tokenUrl, string ssoSharedSecret, int tokenCacheExpirationInMinutes = 50)
        {
            TokenCacheExpirationInMinutes = tokenCacheExpirationInMinutes;
            Scopes = scopes;
            TokenUrl = tokenUrl;
            SharedScret = ssoSharedSecret;

            UseLazyMemoryCache = false;
        }

        /// <summary>
        /// Get a list with custom headers to connect to microservices.
        /// </summary>
        /// <returns></returns>
        protected Dictionary<string, List<string>> GetBearerAuthorizationHeaders()
        {
            return new Dictionary<string, List<string>> { { "Authorization", new List<string> { $"Bearer {GetTokenWithCaching()}" } } };
        }

        protected abstract string GetCacheKey();

        private string GetTokenWithCaching()
        {
            var cacheKey = GetCacheKey();
            if (UseLazyMemoryCache && _memoryCache.TryGetValue(cacheKey, out string accessToken))
            {
                return accessToken;
            }

            accessToken = RequestToken();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                // Set cache options.
                // Keep in cache for this time, reset time if accessed.
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(TokenCacheExpirationInMinutes));

                _memoryCache.Set(cacheKey, accessToken, cacheEntryOptions);
            }

            return accessToken;
        }

        private string RequestToken()
        {
            var client = GetTokenClient();
            var token = client.RequestClientCredentialsAsync(Scopes).Result;
            return token.AccessToken;
        }

        private TokenClient GetTokenClient(string clientId)
        {
            var client = new TokenClient(TokenUrl, clientId, SharedScret);
            return client;
        }
    }
}
