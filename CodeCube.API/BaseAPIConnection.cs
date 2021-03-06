﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;

namespace CodeCube.API
{
    public abstract class BaseApiConnection
    {
        private readonly int _tokenCacheExpirationInMinutes;

        private readonly bool _useLazyMemoryCache;

        private readonly string _scopes;

        private readonly IMemoryCache _memoryCache;
        private readonly TokenClient _tokenClient;

        protected BaseApiConnection(TokenClient tokenClient, IMemoryCache memoryCache, string scopes, int tokenCacheExpirationInMinutes = 50)
        {
            _tokenCacheExpirationInMinutes = tokenCacheExpirationInMinutes;
            _scopes = scopes;
            _useLazyMemoryCache = true;

            _tokenClient = tokenClient;
            _memoryCache = memoryCache;
        }

        protected BaseApiConnection(TokenClient tokenClient, string scopes)
        {
            _tokenClient = tokenClient;

            _scopes = scopes;
            _useLazyMemoryCache = false;
        }

        protected async Task SetBearerToken(HttpClient httpClient)
        {
            Dictionary<string, List<string>> bearerAuthorizationHeader = await GetBearerAuthorizationHeaders();

            if (bearerAuthorizationHeader.TryGetValue("Authorization", out List<string> authorizationHeaders))
            {
                if (authorizationHeaders != null && authorizationHeaders.Count > 0)
                {
                    if (httpClient.DefaultRequestHeaders.Authorization == null)
                    {
                        httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeaders.SingleOrDefault(h => h.StartsWith("Bearer")));
                    }
                    else
                    {
                        httpClient.DefaultRequestHeaders.Remove("Authorization");
                        httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeaders.SingleOrDefault(h => h.StartsWith("Bearer")));
                    }

                    return;
                }
            }

            throw new InvalidOperationException("Authorization headers could not be retrieved!");
        }

        protected abstract string GetCacheKey();

        /// <summary>
        /// Get a list with custom headers to connect to microservices.
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string, List<string>>> GetBearerAuthorizationHeaders()
        {
            return new Dictionary<string, List<string>> { { "Authorization", new List<string> { $"Bearer {await GetTokenWithCaching().ConfigureAwait(false)}" } } };
        }

        private async Task<string> GetTokenWithCaching()
        {
            var cacheKey = GetCacheKey();
            if (_useLazyMemoryCache && _memoryCache.TryGetValue(cacheKey, out string accessToken))
            {
                return accessToken;
            }

            accessToken = await RequestToken().ConfigureAwait(false);

            if (_useLazyMemoryCache && !string.IsNullOrWhiteSpace(accessToken))
            {
                // Set cache options.
                // Keep in cache for this time, reset time if accessed.
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(_tokenCacheExpirationInMinutes));

                _memoryCache.Set(cacheKey, accessToken, cacheEntryOptions);
            }

            return accessToken;
        }

        private async Task<string> RequestToken()
        {
            var token = await _tokenClient.RequestClientCredentialsTokenAsync(_scopes).ConfigureAwait(false);

            return token.AccessToken;
        }
    }
}
