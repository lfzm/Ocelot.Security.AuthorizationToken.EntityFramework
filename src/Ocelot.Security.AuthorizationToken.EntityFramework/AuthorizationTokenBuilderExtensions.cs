using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Security;
using Ocelot.Security.AuthorizationToken;
using Ocelot.Security.AuthorizationToken.Storage;
using System;

namespace Ocelot.DependencyInjection
{
    public static class AuthorizationTokenBuilderExtensions
    {
        public static IOcelotBuilder AddSecurityAuthorizationToken<TDbContent>(this IOcelotBuilder builder,Action<DbContextOptionsBuilder> optionsAction)
            where TDbContent:DbContext 
        {
            builder.Services.AddSingleton<IAuthorizationTokenStorage, AuthorizationTokenStorage>();
            builder.Services.AddSingleton<ISecurityPolicy, AuthorizationTokenSecurityPolicy>();
            builder.Services.AddDbContext<OcelotSecurityStorageDbContent>(optionsAction);
            return builder;
        }

    }
}
