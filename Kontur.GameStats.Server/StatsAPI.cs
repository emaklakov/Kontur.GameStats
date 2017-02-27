using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Kontur.GameStats.Server.Models;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server
{
    public class StatsAPI
    {
        public readonly WorkDB workDb;

        public StatsAPI()
        {
            workDb = new WorkDB();
        }

        public void GetServersInfo(HttpListenerContext listenerContext)
        {
            try
            {
                EndpointInfo[] servers = workDb.GetServersInfo();

                string serversJson = JsonConvert.SerializeObject(servers);

                listenerContext.Response.StatusCode = (int) HttpStatusCode.OK;
                listenerContext.Response.ContentType = "application/json";
                using (var writer = new System.IO.StreamWriter(listenerContext.Response.OutputStream))
                {
                    writer.WriteLine(serversJson);
                }
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, error.Message));
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                listenerContext.Response.Close();
            }
        }

        public void GetServerInfo(HttpListenerContext listenerContext)
        {
            try
            {
                string endpoint = ExtractEndpoint(listenerContext.Request);
                ServerInfo serverInfo = workDb.GetServerInfo(endpoint);

                if (serverInfo == null)
                {
                    listenerContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    listenerContext.Response.Close();
                    return;
                }

                string serverInfoJson = JsonConvert.SerializeObject(serverInfo);

                listenerContext.Response.StatusCode = (int) HttpStatusCode.OK;
                listenerContext.Response.ContentType = "application/json";
                using (var writer = new System.IO.StreamWriter(listenerContext.Response.OutputStream))
                {
                    writer.Write(serverInfoJson);
                }
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, error.Message));
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                listenerContext.Response.Close();
            }
        }

        public void PutServerInfo(HttpListenerContext listenerContext)
        {
            try
            {
                var inpStream = new StreamReader(listenerContext.Request.InputStream);

                ServerInfo serverInfo = JsonConvert.DeserializeObject<ServerInfo>(inpStream.ReadToEnd());
                string endPoint = ExtractEndpoint(listenerContext.Request);

                if (workDb.PutServerInfo(new EndpointInfo(endPoint, serverInfo)))
                {
                    listenerContext.Response.StatusCode = (int) HttpStatusCode.OK;
                }
                else
                {
                    listenerContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                }
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, error.Message));
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            }

            listenerContext.Response.Close();
        }

        public void GetServerStats(HttpListenerContext listenerContext)
        {
            try
            {
                string endpoint = ExtractEndpoint(listenerContext.Request);

                ServerStats serverStats = workDb.GetServerStats(endpoint);

                if (serverStats == null)
                {
                    listenerContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    listenerContext.Response.Close();
                    return;
                }

                string statsJson = JsonConvert.SerializeObject(serverStats);

                listenerContext.Response.StatusCode = (int) HttpStatusCode.OK;
                listenerContext.Response.ContentType = "application/json";
                using (var writer = new System.IO.StreamWriter(listenerContext.Response.OutputStream))
                {
                    writer.WriteLine(statsJson);
                }
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, error.Message));
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                listenerContext.Response.Close();
            }
        }

        public void GetServerMatch(HttpListenerContext listenerContext)
        {
            try
            {
                string endpoint = ExtractEndpoint(listenerContext.Request);
                DateTime timestamp = DateTimeOffset.Parse(ExtractTimestamp(listenerContext.Request)).UtcDateTime;

                MatchInfo matchInfo = workDb.GetServerMatch(endpoint, timestamp);

                if (matchInfo == null)
                {
                    listenerContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    listenerContext.Response.Close();
                    return;
                }

                string matchInfoJson = JsonConvert.SerializeObject(matchInfo);

                listenerContext.Response.StatusCode = (int) HttpStatusCode.OK;
                listenerContext.Response.ContentType = "application/json";
                using (var writer = new System.IO.StreamWriter(listenerContext.Response.OutputStream))
                {
                    writer.Write(matchInfoJson);
                }
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, error.Message));
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                listenerContext.Response.Close();
            }
        }

        public void PutServerMatch(HttpListenerContext listenerContext)
        {
            var inpStream = new StreamReader(listenerContext.Request.InputStream);

            string endpoint = ExtractEndpoint(listenerContext.Request);

            if (workDb.IsExistServer(endpoint))
            {
                try
                {
                    DateTime timestamp = DateTimeOffset.Parse(ExtractTimestamp(listenerContext.Request)).UtcDateTime;

                    MatchInfo matchInfo = JsonConvert.DeserializeObject<MatchInfo>(inpStream.ReadToEnd());

                    if (workDb.PutServerMatch(endpoint, timestamp, matchInfo))
                    {
                        listenerContext.Response.StatusCode = (int) HttpStatusCode.OK;
                    }
                    else
                    {
                        listenerContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                    }
                }
                catch (Exception error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, error.Message));
                    Console.ResetColor();
                    listenerContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                listenerContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }

            listenerContext.Response.Close();
        }

        public void GetBestPlayersReport(HttpListenerContext listenerContext)
        {
            try
            {
                int countPlayers = ExtractCountPlayers(listenerContext.Request);

                BestPlayer[] bestPlayers = workDb.GetBestPlayersReport(countPlayers);

                string reportJson = JsonConvert.SerializeObject(bestPlayers);

                listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                listenerContext.Response.ContentType = "application/json";
                using (var writer = new System.IO.StreamWriter(listenerContext.Response.OutputStream))
                {
                    writer.WriteLine(reportJson);
                }
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, error.Message));
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                listenerContext.Response.Close();
            }  
        }

        public void HandleIncorrect(HttpListenerContext listenerContext)
        {
            listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            listenerContext.Response.Close();
        }

        private static string ExtractEndpoint(HttpListenerRequest request)
        {
            return request.RawUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1];
        }

        private static string ExtractTimestamp(HttpListenerRequest request)
        {
            return request.RawUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[3];
        }

        private static int ExtractCountPlayers(HttpListenerRequest request)
        {
            string[] par = request.RawUrl.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            string count = par.Length > 2 ? request.RawUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[2] : "0";

            return Int32.Parse(count);
        }
    }
}
