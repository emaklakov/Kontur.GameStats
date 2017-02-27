using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.Models
{
    public class MatchInfoStat
    {
        public DateTime timeStamp { get; set; }
        public string map { get; set; }
        public string gameMode { get; set; }
        public int fragLimit { get; set; }
        public int timeLimit { get; set; }
        public double timeElapsed { get; set; } 

        public MatchInfoStat(DateTime timeStamp, string map, string gameMode, int fragLimit, int timeLimit, double timeElapsed)
        {
            this.timeStamp = timeStamp;
            this.map = map;
            this.gameMode = gameMode;
            this.fragLimit = fragLimit;
            this.timeLimit = timeLimit;
            this.timeElapsed = timeElapsed;
        } 
    }
}
