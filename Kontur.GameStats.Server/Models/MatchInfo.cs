using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.Models
{
    public class MatchInfo
    {
        public string map;
        public string gameMode;
        public int fragLimit;
        public int timeLimit;
        public double timeElapsed;
        public ScoreboardItem[] scoreboard;

        public class ScoreboardItem
        {
            public string name;
            public int frags;
            public int kills;
            public int deaths;
        }

        public MatchInfo()
        {

        }

    }
}
