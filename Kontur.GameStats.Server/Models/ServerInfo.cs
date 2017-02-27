using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.Models
{
    public class ServerInfo
    {        
        public string name { get; set; }
        public string[] gameModes { get; set; }

        public ServerInfo(string name, string[] gamemodes)
        {
            this.name = name;
            gameModes = gamemodes;
        }

        public string GetGameModesString()
        {
            return string.Join(",", gameModes);
        }
    }
}
