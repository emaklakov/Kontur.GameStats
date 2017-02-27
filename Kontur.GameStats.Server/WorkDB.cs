using System;
using System.Collections.Generic;
using System.Configuration;
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

            SqlCommand dbCommand = new SqlCommand("SELECT EndPoint, Name, GameModes FROM Servers", dbConnection);
            SqlDataReader reader = dbCommand.ExecuteReader();

            while (reader.Read())
            {
                string endpoint = (string)reader["endpoint"];
                string name = (string)reader["name"];
                string gamemodes = (string)reader["gamemodes"];

                servers.Add(new EndpointInfo(endpoint, new ServerInfo(name, gamemodes.Split(','))));
            }
            reader.Close();

            return servers.ToArray();
        }
    }
}
