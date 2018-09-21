using Microsoft.Extensions.DependencyInjection;
using Ocelot.Security;
using Ocelot.Security.AuthorizationToken;
using Ocelot.Security.AuthorizationToken.Storage;

namespace Ocelot.DependencyInjection
{
    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddSecurityAuthorizationToken(this IOcelotBuilder builder)
        {
            //builder.Services.AddSingleton<OcelotSecurityStorageDbContent>();
            builder.Services.AddSingleton<IAuthorizationTokenStorage, AuthorizationTokenStorage>();
            builder.Services.AddSingleton<ISecurityPolicy, AuthorizationTokenSecurityPolicy>();
            return builder;
        }

    }
}
