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

namespace Ocelot.Security.AuthorizationToken.EntityFramework.Test
{
    public class AuthorizationTokenSecurityPolicyTests
    {
        private readonly Mock<IAuthorizationTokenStorage> _storage;
        private readonly Mock<IMemoryCache> _cache;
        private readonly Mock<ILogger<AuthorizationTokenSecurityPolicy>> _logger;
        private readonly AuthorizationTokenSecurityPolicy _securityPolicy;
        private readonly DownstreamContext _downstreamContext;
        private readonly string toekn = "1234567890";
        private Response response;

        public AuthorizationTokenSecurityPolicyTests()
        {
            _storage = new Mock<IAuthorizationTokenStorage>();
            _cache = new Mock<IMemoryCache>();
            _logger = new Mock<ILogger<AuthorizationTokenSecurityPolicy>>();
            _securityPolicy = new AuthorizationTokenSecurityPolicy(_cache.Object, _storage.Object, _logger.Object);
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
            this.Given(x => x.GivenGetCacheNotExisteBlackList())
                .When(x => x.WhenSecurity())
                .Then(f => f.ThenSecurityPassing())
                .BDDfy();
        }


        private void GivenGetCacheExisteBlackList()
        {
            object o="exist_token";
            _cache.Setup(x => x.TryGetValue(toekn,out o)).Returns(true);
        }
        private void GivenGetCacheNotExisteBlackList()
        {
            object result;
            _cache.Setup(x => x.TryGetValue(toekn, out result)).Returns(false);
        }

        private void WhenSecurity()
        {
            response = _securityPolicy.Security(_downstreamContext).GetAwaiter().GetResult();
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
    }
}
