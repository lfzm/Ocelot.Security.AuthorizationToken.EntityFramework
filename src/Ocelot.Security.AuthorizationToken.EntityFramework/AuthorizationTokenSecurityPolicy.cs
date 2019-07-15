using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Ocelot.Middleware;
using Ocelot.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.Security.AuthorizationToken
{
    public class AuthorizationTokenSecurityPolicy : ISecurityPolicy
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        private readonly IAuthorizationTokenStorage _storage;
        private DateTime refreshTime = DateTime.Now.AddYears(-5);
        private int isProcessing = 0;
        private int tokenRefreshInterval = 1000;
        private long lastId = 0;
        public AuthorizationTokenSecurityPolicy(IMemoryCache cache, IAuthorizationTokenStorage storage, ILogger<AuthorizationTokenSecurityPolicy> logger)
        {
            _cache = cache;
            _storage = storage;
            _logger = logger;
        }

        public async Task<Response> Security(DownstreamContext context)
        {
            string authorization = context.HttpContext.Request.Headers["Authorization"];

            // If no authorization header found, nothing to process further
            if (string.IsNullOrEmpty(authorization))
            {
                return new OkResponse();
            }
            string tokne = string.Empty;
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                tokne = authorization.Substring("Bearer ".Length).Trim();
            }
            this.LoadBlacklistToken();

            if (_cache.TryGetValue(tokne, out string warnInfo))
            {
                context.HttpContext.Response.StatusCode = 401;
                var bytes = Encoding.UTF8.GetBytes(warnInfo);
                await context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                var error = new UnauthenticatedError($"{tokne} Token enters the blacklist");
                return new ErrorResponse(error);
            }
            else
                return await Task.FromResult(new OkResponse());
        }

        private void LoadBlacklistToken()
        {
            if ((DateTime.Now - refreshTime).TotalMilliseconds < tokenRefreshInterval * 2)
            {
                return;
            }
            if (Interlocked.CompareExchange(ref isProcessing, 1, 0) == 1)
                return;
            new Task(async () =>
            {
                while (true)
                {
                    try
                    {
                        refreshTime = DateTime.Now;
                        Task.Delay(tokenRefreshInterval).Wait();
                        IList<AuthorizationToken> tokens = await _storage.GetList(lastId);
                        if (tokens == null || tokens.Count == 0)
                            continue;
                        foreach (var item in tokens)
                        {
                            if (_cache.TryGetValue(item.Token, out string warnInfo))
                                continue;

                            _cache.Set(item.Token, item.WarnInfo, new MemoryCacheEntryOptions()
                            {
                                AbsoluteExpiration = item.Expiration
                            });
                        }
                        lastId = tokens.LastOrDefault().Id;
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, "Loading AuthorizationToken blacklist failed");
                    }
                }
            }).Start();
        }
    }
}
