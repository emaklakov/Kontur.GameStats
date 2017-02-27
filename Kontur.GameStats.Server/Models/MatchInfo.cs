using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.Models
{
    public class MatchInfo
    {
        public string map { get; set; }
        public string gameMode { get; set; }
        public int fragLimit { get; set; }
        public int timeLimit { get; set; }
        public double timeElapsed { get; set; }
        public ScoreboardItem[] scoreboard { get; set; }

        public class ScoreboardItem
        {
            public string name { get; set; }
            public int frags { get; set; }
            public int kills { get; set; }
            public int deaths { get; set; }

            public ScoreboardItem(string name, int frags, int kills, int deaths)
            {
                this.name = name;
                this.frags = frags;
                this.kills = kills;
                this.deaths = deaths;
            }
        }

        public MatchInfo(string map, string gameMode, int fragLimit, int timeLimit, double timeElapsed)
        {
            this.map = map;
            this.gameMode = gameMode;
            this.fragLimit = fragLimit;
            this.timeLimit = timeLimit;
            this.timeElapsed = timeElapsed;
        } 
    }
}
