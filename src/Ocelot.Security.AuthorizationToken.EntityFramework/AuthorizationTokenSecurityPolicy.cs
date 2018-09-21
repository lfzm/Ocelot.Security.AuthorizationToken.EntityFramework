using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Ocelot.Middleware;
using Ocelot.Responses;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Ocelot.Security.AuthorizationToken
{
    public class AuthorizationTokenSecurityPolicy : ISecurityPolicy
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        private readonly IAuthorizationTokenStorage _storage;
        private DateTime refreshTime = DateTime.Now.AddYears(-5);
        private int isProcessing = 0;
        private int tokenRefreshInterval = 5000;
        public AuthorizationTokenSecurityPolicy(IMemoryCache cache, IAuthorizationTokenStorage storage, ILogger<AuthorizationTokenSecurityPolicy> logger)
        {
            _cache = cache;
            _storage = storage;
            _logger = logger;
        }

        public async Task<Response> Security(DownstreamContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues value))
                return new OkResponse();
            string tokne = value.ToString().Replace("Bearer ", "");

            this.LoadBlacklistToken();

            if (_cache.TryGetValue(tokne, out AuthorizationToken tokenModel))
            {
                //context.HttpContext.Response.StatusCode = 401;
                //var bytes = Encoding.UTF8.GetBytes(tokenModel.WarnInfo);
                //await context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                var error = new UnauthenticatedError($"{tokne} Token enters the blacklist");
                return new ErrorResponse(error);
            }
            else
                return new OkResponse();
        }

        private void LoadBlacklistToken()
        {
            if ((DateTime.Now - refreshTime).TotalMilliseconds < tokenRefreshInterval * 2)
            {
                return;
            }
            if (Interlocked.CompareExchange(ref isProcessing, 1, 0) == 1)
                return;
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        DateTime _rtime = refreshTime;
                        refreshTime = DateTime.Now;
                        Task.Delay(tokenRefreshInterval).Wait();
                        IList<AuthorizationToken> tokens = await _storage.GetList(_rtime);
                        if (tokens == null || tokens.Count == 0)
                            continue;
                        foreach (var item in tokens)
                        {
                            if (_cache.TryGetValue(item.Token, out AuthorizationToken model))
                                continue;

                            _cache.Set(item.Token, item, new MemoryCacheEntryOptions()
                            {
                                AbsoluteExpiration = item.Expiration
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Loading AuthorizationToken blacklist failed");
                }
                finally
                {
                    Interlocked.Exchange(ref isProcessing, 0);
                }
            }).ConfigureAwait(false);
        }
    }
}
