using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.Models
{
    public class EndpointInfo
    {
        public string endpoint { get; set; }
        public ServerInfo info { get; set; }

        public EndpointInfo(string endpoint, ServerInfo serverInfo)
        {
            this.endpoint = endpoint;
            info = serverInfo;
        }
    }
}
