using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.Security.AuthorizationToken
{
    public interface IAuthorizationTokenStorage
    {
        Task<List<AuthorizationToken>> GetList(long lastId);
    }
}
