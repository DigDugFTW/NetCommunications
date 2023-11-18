using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServerWindowsForms.Models
{
    public class ServerConfigurationModel
    {
        public string Password { get; set; } = "helloworld";

        public bool ForceClientLimit { get; set; } = false;

        public int ClientLimit { get; set; }

        public int ListeningPort { get; set; } = 1337;

    }
}
