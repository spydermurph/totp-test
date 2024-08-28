using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.DTOs
{
    public class UserToken
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsTwoFactor { get; set; }
        public string Provider { get; set; }
        public string Email { get; set; }
        public bool IsRegistered { get; set; }
        public string TokenUri { get; set; }
    }
}
