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

            dbConnection.Open();
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

            dbConnection.Open();
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
            
            dbConnection.Open();

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

        public bool IsExistServer(string endpoint)
        {
            bool IsExist = false;

            dbConnection.Open();

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
    }
}
