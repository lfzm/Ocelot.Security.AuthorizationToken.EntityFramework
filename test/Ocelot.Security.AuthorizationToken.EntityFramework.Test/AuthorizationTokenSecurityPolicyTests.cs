using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using TestStack.BDDfy;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Ocelot.Request.Middleware;
using Ocelot.Responses;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Ocelot.Security.AuthorizationToken.EntityFramework.Test
{
    public class AuthorizationTokenSecurityPolicyTests
    {
        private readonly Mock<IAuthorizationTokenStorage> _storage;
        private readonly IMemoryCache _cache;
        private readonly Mock<ILogger<AuthorizationTokenSecurityPolicy>> _logger;
        private readonly AuthorizationTokenSecurityPolicy _securityPolicy;
        private readonly DownstreamContext _downstreamContext;
        private readonly string toekn = "1234567890";
        private Response response;

        public AuthorizationTokenSecurityPolicyTests()
        {
            _cache = new MemoryCache(Options.Create<MemoryCacheOptions>(new MemoryCacheOptions()));
            _storage = new Mock<IAuthorizationTokenStorage>();
            _logger = new Mock<ILogger<AuthorizationTokenSecurityPolicy>>();
            _securityPolicy = new AuthorizationTokenSecurityPolicy(_cache, _storage.Object, _logger.Object);
            _downstreamContext = new DownstreamContext(new DefaultHttpContext());
            _downstreamContext.DownstreamRequest = new DownstreamRequest(new HttpRequestMessage(HttpMethod.Get, "http://test.com"));
            _downstreamContext.HttpContext.Request.Headers.Add("Authorization", toekn);
        }

        [Fact]
        public void should_token_existe_blacklist()
        {
            this.Given(x => x.GivenGetCacheExisteBlackList())
                .When(x => x.WhenSecurity())
                .Then(f => f.ThenSecurityNotPassing())
                .BDDfy();
        }

        [Fact]
        public void should_token_not_existe_blacklist()
        {
            this
                .When(x => x.WhenSecurity())
                .Then(f => f.ThenSecurityPassing())
                .BDDfy();
        }

        [Fact]
        public void should_token_blacklis_disappear()
        {
            this.Given(x => x.GivenStorage())
                .When(x => x.WhenSecurityWait(5000))
                .Then(x => x.ThenTokenBlacklisIsDisappear())
                .BDDfy();
        }

        private void GivenGetCacheExisteBlackList()
        {
            _cache.Set(toekn, "exist_token", new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddHours(1)
            });
        }
     
        private void GivenStorage()
        {
            List<AuthorizationToken> tokens = new List<AuthorizationToken>();
            tokens.Add(new AuthorizationToken()
            {
                AddTime = DateTime.Now,
                Expiration = DateTime.Now.AddMilliseconds(1000),
                Id = 1,
                Token = "1",
                WarnInfo = "123"
            });
            tokens.Add(new AuthorizationToken()
            {
                AddTime = DateTime.Now,
                Expiration = DateTime.Now.AddHours(1),
                Id = 1,
                Token = "2",
                WarnInfo = "123"
            });
            _storage.Setup(x => x.GetList(0)).Returns(Task.FromResult(tokens));

        }
        private void WhenSecurity()
        {
            response = _securityPolicy.Security(_downstreamContext).GetAwaiter().GetResult();
        }

        private void WhenSecurityWait(int millisecondsDelay)
        {
            response = _securityPolicy.Security(_downstreamContext).GetAwaiter().GetResult();
            Task.Delay(millisecondsDelay).Wait();
        }

        private void ThenSecurityPassing()
        {
            Assert.False(response.IsError);
        }
        private void ThenSecurityNotPassing()
        {
            Assert.Equal(401, _downstreamContext.HttpContext.Response.StatusCode);
            Assert.True(response.IsError);
        }
        private void ThenTokenBlacklisIsDisappear()
        {
            Assert.False(_cache.TryGetValue("1", out string a));
            Assert.True(_cache.TryGetValue("2", out string b));
            Assert.False(response.IsError);
        }
    }
}
