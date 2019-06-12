using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.Security.AuthorizationToken.Storage
{
    public class AuthorizationTokenStorage : IAuthorizationTokenStorage
    {
        private readonly OcelotSecurityStorageDbContent _dbContent;

        public AuthorizationTokenStorage(OcelotSecurityStorageDbContent dbContent)
        {
            this._dbContent = dbContent;
        }

        public Task<List<AuthorizationToken>> GetList(long lastId)
        {
            this._dbContent.Database.ExecuteSqlCommand("DELETE FROM Ocelot_SecurityToken WHERE  Expiration< '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff ") + "'");
            return _dbContent.AuthorizationTokens.Where(f => f.Id >= lastId).ToListAsync();
        }
    }
}
