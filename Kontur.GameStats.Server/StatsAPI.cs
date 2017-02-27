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

                listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
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
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    writer.WriteLine(String.Empty);
            }
            
        }

        public void GetBestPlayersReport(HttpListenerContext listenerContext)
        {
            string reportJson = "";

            listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
            using (var writer = new System.IO.StreamWriter(listenerContext.Response.OutputStream))
            {
                writer.WriteLine(reportJson);
            }
        }

        public void HandleIncorrect(HttpListenerContext listenerContext)
        {
            listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
    }
}
