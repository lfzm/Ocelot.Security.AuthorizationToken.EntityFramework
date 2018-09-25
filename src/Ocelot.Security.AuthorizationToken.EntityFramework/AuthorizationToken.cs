using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.Security.AuthorizationToken
{
    public class AuthorizationToken
    {
        public long Id { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string WarnInfo { get; set; }
        public DateTime AddTime { get; set; }
    }
}
