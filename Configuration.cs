using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DynamoPMCLI
{
    public class Configuration
    {
        public string Source { get; set; }
        public string ClientID { get; set; }
        public string Token { get; set; }
        public string Environment { get; set; }
    }
}
