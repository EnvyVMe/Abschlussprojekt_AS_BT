using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildToolService.DataTypes
{
    public class UserSession
    {
        public string UserId { get; set; }
        public string SessionKey { get; set; }
    }
}