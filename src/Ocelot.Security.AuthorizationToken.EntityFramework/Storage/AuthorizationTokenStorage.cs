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
            var entitys = _dbContent.AuthorizationTokens.Where(p => p.Expiration < DateTime.Now);
            _dbContent.AuthorizationTokens.RemoveRange(entitys);
            _dbContent.SaveChanges();

            return _dbContent.AuthorizationTokens.Where(f => f.Id >= lastId).ToListAsync();
        }
    }
}
