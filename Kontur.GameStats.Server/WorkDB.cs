using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontur.GameStats.Server.Models;

namespace Kontur.GameStats.Server
{
    /// <summary>
    /// Работа с БД
    /// </summary>
    public class WorkDB
    {
        private string connectionString = "";
        private SqlConnection dbConnection;

        public WorkDB()
        {
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                dbConnection = new SqlConnection(connectionString); 
                dbConnection.Open();
                dbConnection.Close();
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, error.Message));
                Console.ResetColor();
            }
        }

        public EndpointInfo[] GetServersInfo()
        {
            List<EndpointInfo> servers = new List<EndpointInfo>();

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            SqlCommand dbCommand = new SqlCommand("SELECT EndPoint, Name, GameModes FROM Servers", dbConnection);
            SqlDataReader reader = dbCommand.ExecuteReader();

            while (reader.Read())
            {
                string endpoint = (string)reader["endpoint"];
                string name = (string)reader["name"];
                string gamemodes = (string)reader["gamemodes"];

                servers.Add(new EndpointInfo(endpoint.Trim(), new ServerInfo(name.Trim(), gamemodes.Trim().Split(','))));
            }
            reader.Close();

            dbConnection.Close();

            return servers.ToArray();
        }

        public ServerInfo GetServerInfo(string endpoint)
        {
            ServerInfo serverInfo = null;

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            SqlCommand dbCommand = new SqlCommand("SELECT Name, GameModes FROM Servers WHERE EndPoint = @EndPoint", dbConnection);
            dbCommand.Parameters.AddWithValue("@EndPoint", endpoint);
            SqlDataReader reader = dbCommand.ExecuteReader();
            while (reader.Read())
            {
                string name = (string)reader["name"];
                string gamemodes = (string)reader["gamemodes"];

                serverInfo = new ServerInfo(name.Trim(), gamemodes.Trim().Split(','));
                break;
            }
            reader.Close();

            dbConnection.Close();

            return serverInfo;
        }

        public bool PutServerInfo(EndpointInfo server)
        {
            bool result = false;  

            string SQL = "INSERT INTO Servers VALUES (@EndPoint, @Name, @GameModes)";

            if (IsExistServer(server.endpoint))
            {
                SQL = "UPDATE Servers SET Name=@Name, GameModes=@GameModes WHERE EndPoint=@EndPoint";
            }

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            SqlCommand dbCommand = new SqlCommand(SQL, dbConnection);
            dbCommand.Parameters.AddWithValue("@EndPoint", server.endpoint);
            dbCommand.Parameters.AddWithValue("@Name", server.info.name);
            dbCommand.Parameters.AddWithValue("@GameModes", server.info.GetGameModesString());

            int rows = -1;
            rows = dbCommand.ExecuteNonQuery();
            if (rows >= 0)
            {
                result = true;
            }

            dbConnection.Close();

            return result;
        }

        public MatchInfo GetServerMatch(string endpoint, DateTime timestamp)
        {
            MatchInfo matchInfo = null;

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            SqlCommand dbCommand = new SqlCommand("SELECT * FROM Matches WHERE EndPoint = @EndPoint AND TimeStamp = @TimeStamp", dbConnection);
            dbCommand.Parameters.AddWithValue("@EndPoint", endpoint);
            dbCommand.Parameters.AddWithValue("@TimeStamp", timestamp);
            SqlDataReader reader = dbCommand.ExecuteReader();
            Guid MatchId = Guid.Empty;
            while (reader.Read())
            {
                MatchId = (Guid)reader["Id"];
                string map = (string)reader["Map"];
                string gameMode = (string)reader["GameMode"];
                int fragLimit = (int)reader["FragLimit"];
                int timeLimit = (int)reader["TimeLimit"];
                double timeElapsed = (double)reader["TimeElapsed"];

                matchInfo = new MatchInfo(map.Trim(), gameMode.Trim(), fragLimit, timeLimit, timeElapsed);
                break;
            }
            reader.Close();

            if (MatchId != Guid.Empty)
            {
                dbCommand = new SqlCommand("SELECT * FROM Scoreboards WHERE MatchId = @MatchId", dbConnection);
                dbCommand.Parameters.AddWithValue("@MatchId", MatchId);

                reader = dbCommand.ExecuteReader();

                List<MatchInfo.ScoreboardItem> scoreboardItems = new List<MatchInfo.ScoreboardItem>();

                while (reader.Read())
                {
                    string name = (string)reader["Name"];
                    int frags = (int)reader["Frags"];
                    int kills = (int)reader["Kills"];
                    int deaths = (int)reader["Deaths"];

                    scoreboardItems.Add(new MatchInfo.ScoreboardItem(name.Trim(), frags, kills, deaths));
                }
                reader.Close();

                matchInfo.scoreboard = scoreboardItems.ToArray();
            }

            dbConnection.Close();

            return matchInfo;
        }

        public bool PutServerMatch(string endpoint, DateTime timestamp, MatchInfo match)
        {
            bool result = false;

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            SqlCommand dbCommand = new SqlCommand("INSERT INTO Matches VALUES (@Id, @EndPoint, @TimeStampt, @Map, @GameMode, @FragLimit, @TimeLimit, @TimeElapsed)", dbConnection);
            Guid MatchId = Guid.NewGuid();
            dbCommand.Parameters.AddWithValue("@Id", MatchId);
            dbCommand.Parameters.AddWithValue("@EndPoint", endpoint);
            dbCommand.Parameters.AddWithValue("@TimeStampt", timestamp);
            dbCommand.Parameters.AddWithValue("@Map", match.map);
            dbCommand.Parameters.AddWithValue("@GameMode", match.gameMode);
            dbCommand.Parameters.AddWithValue("@FragLimit", match.fragLimit);
            dbCommand.Parameters.AddWithValue("@TimeLimit", match.timeLimit);
            dbCommand.Parameters.AddWithValue("@TimeElapsed", match.timeElapsed);

            int rows = -1;
            rows = dbCommand.ExecuteNonQuery();
            if (rows >= 0)
            {
                foreach (MatchInfo.ScoreboardItem scoreboardItem in match.scoreboard)
                {
                    dbCommand = new SqlCommand("INSERT INTO Scoreboards VALUES (@MatchId, @Name, @Frags, @Kills, @Deaths)", dbConnection);
                    dbCommand.Parameters.AddWithValue("@MatchId", MatchId);
                    dbCommand.Parameters.AddWithValue("@Name", scoreboardItem.name);
                    dbCommand.Parameters.AddWithValue("@Frags", scoreboardItem.frags);
                    dbCommand.Parameters.AddWithValue("@Kills", scoreboardItem.kills);
                    dbCommand.Parameters.AddWithValue("@Deaths", scoreboardItem.deaths);
                    dbCommand.ExecuteNonQuery();
                }

                result = true;
            }

            dbConnection.Close();

            return result;
        }

        public ServerStats GetServerStats(string endpoint)
        {
            ServerStats serverStats = null;

            List<MatchInfoStat> matches = new List<MatchInfoStat>();
            List<MatchInfo.ScoreboardItem> scoreboards = new List<MatchInfo.ScoreboardItem>();

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            SqlCommand dbCommand = new SqlCommand("SELECT * FROM Matches WHERE EndPoint = @EndPoint", dbConnection);
            dbCommand.Parameters.AddWithValue("@EndPoint", endpoint);

            SqlDataReader reader = dbCommand.ExecuteReader();
            Guid MatchId = Guid.Empty;
            while (reader.Read())
            {
                MatchId = (Guid)reader["Id"];
                DateTime timeStamp = (DateTime)reader["TimeStamp"];
                string map = (string)reader["Map"];
                string gameMode = (string)reader["GameMode"];
                int fragLimit = (int)reader["FragLimit"];
                int timeLimit = (int)reader["TimeLimit"];
                double timeElapsed = (double)reader["TimeElapsed"];

                MatchInfoStat matchInfo = new MatchInfoStat(timeStamp, map.Trim(), gameMode.Trim(), fragLimit, timeLimit, timeElapsed);
                matches.Add(matchInfo);
            }
            reader.Close();

            if (MatchId != Guid.Empty)
            {
                dbCommand = new SqlCommand("SELECT * FROM Scoreboards WHERE MatchId = @MatchId", dbConnection);
                dbCommand.Parameters.AddWithValue("@MatchId", MatchId);

                reader = dbCommand.ExecuteReader(); 

                while (reader.Read())
                {
                    string name = (string)reader["Name"];
                    int frags = (int)reader["Frags"];
                    int kills = (int)reader["Kills"];
                    int deaths = (int)reader["Deaths"];

                    scoreboards.Add(new MatchInfo.ScoreboardItem(name.Trim(), frags, kills, deaths));
                }
                reader.Close();
            }

            var matchesCountGroupByDate = matches
                        .GroupBy(match => new DateTime(match.timeStamp.Year, match.timeStamp.Month, match.timeStamp.Day))
                        .Select(group => group.Count());

            var playersCountGroupByName = scoreboards
                        .GroupBy(scoreboard => scoreboard.name)
                        .Select(group => group.Count());

            serverStats = new ServerStats()
            {
                totalMatchesPlayed = matches.Count,
                maximumMatchesPerDay = matchesCountGroupByDate.Count() == 0 ? 0 : matchesCountGroupByDate.Max(),
                averageMatchesPerDay = matchesCountGroupByDate.Count() == 0 ? 0 : matchesCountGroupByDate.Average(),
                maximumPopulation = playersCountGroupByName.Count() == 0 ? 0 : playersCountGroupByName.Max(),
                averagePopulation = playersCountGroupByName.Count() == 0 ? 0 : playersCountGroupByName.Average(),
                top5GameModes = GetTop5GameModes(matches),
                top5Maps = GetTop5Maps(matches)
            };

            dbConnection.Close();

            return serverStats;
        }

        private static string[] GetTop5GameModes(List<MatchInfoStat> matches)
        {
            return matches
                .GroupBy(match => match.gameMode)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .Take(5)
                .ToArray();
        }

        private static string[] GetTop5Maps(List<MatchInfoStat> matches)
        {
            return matches
                .GroupBy(match => match.map)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .Take(5)
                .ToArray();
        }

        public bool IsExistServer(string endpoint)
        {
            bool IsExist = false;

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }     

            SqlCommand dbCommand = new SqlCommand("SELECT EndPoint FROM Servers WHERE EndPoint=@EndPoint", dbConnection);
            dbCommand.Parameters.AddWithValue("@EndPoint", endpoint);

            SqlDataReader reader = dbCommand.ExecuteReader();

            while (reader.Read())
            {
                IsExist = true;
            }

            reader.Close();

            dbConnection.Close();

            return IsExist;
        }

        public BestPlayer[] GetBestPlayersReport(int countPlayers)
        {
            List<BestPlayer> players = new List<BestPlayer>();
            List<MatchInfo.ScoreboardItem> scoreboards = new List<MatchInfo.ScoreboardItem>();

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            SqlCommand dbCommand = new SqlCommand("SELECT * FROM Scoreboards WHERE Frags >= 10 AND Kills > 0", dbConnection);
            SqlDataReader reader = dbCommand.ExecuteReader(); 

            while (reader.Read())
            {
                string name = (string)reader["Name"];
                int frags = (int)reader["Frags"];
                int kills = (int)reader["Kills"];
                int deaths = (int)reader["Deaths"];

                scoreboards.Add(new MatchInfo.ScoreboardItem(name.Trim(), frags, kills, deaths));
            }
            reader.Close();

            dbConnection.Close();

            var playersCountGroupByName = scoreboards
                        .GroupBy(scoreboard => scoreboard.name)
                        .Select(group => new
                        {
                            name = group.Key,
                            totalKills = group.Sum(s=> s.kills),
                            totalDeaths = group.Sum(s => s.deaths)
                        });

            foreach (var player in playersCountGroupByName)
            {
                BestPlayer bestPlayer = new BestPlayer()
                {
                    name = player.name,
                    killToDeathRatio = player.totalKills / player.totalDeaths
                };

                players.Add(bestPlayer);
            }

            return players.ToArray();
        }
    }
}
