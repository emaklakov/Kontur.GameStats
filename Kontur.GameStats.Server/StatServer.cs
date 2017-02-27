using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server
{
    internal class StatServer : IDisposable
    {
        public StatServer()
        {
            listener = new HttpListener();
            statsApi = new StatsAPI();
        }
        
        public void Start(string prefix)
        {
            lock (listener)
            {
                if (!isRunning)
                {
                    listener.Prefixes.Clear();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    listenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    listenerThread.Start();

                    isRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;

                listener.Stop();

                listenerThread.Abort();
                listenerThread.Join();
                
                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            listener.Close();
        }
        
        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (listener.IsListening)
                    {
                        var context = listener.GetContext();
                        Task.Run(() => HandleContextAsync(context));
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    // TODO: log errors
                }
            }
        }

        private async Task HandleContextAsync(HttpListenerContext listenerContext)
        {
            // TODO: implement request handling

            //Объект запроса
            HttpListenerRequest request = listenerContext.Request;

            Console.WriteLine("{1} {0}", request.RawUrl, request.HttpMethod);

            string[] address = request.RawUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (address.Length > 0)
            {
                if (address[0] == "servers")
                {
                    if (address[1] == "info")
                    {
                        // /servers/info GET   
                        if (request.HttpMethod == System.Net.Http.HttpMethod.Get.Method)
                        {
                            statsApi.GetServersInfo(listenerContext);
                        }          
                        else
                        {
                            statsApi.HandleIncorrect(listenerContext);
                        }   
                    }
                    else
                    {
                        switch (address[2])
                        {
                            case "info":
                                // /servers/<endpoint>/info PUT, GET
                                if (request.HttpMethod == System.Net.Http.HttpMethod.Get.Method)
                                    statsApi.GetServerInfo(listenerContext);
                                else if (request.HttpMethod == System.Net.Http.HttpMethod.Put.Method)
                                    statsApi.PutServerInfo(listenerContext);
                                else
                                    statsApi.HandleIncorrect(listenerContext);
                                break;
                            default:
                                statsApi.HandleIncorrect(listenerContext);
                                break;
                        }
                    }
                }
                else if (address[0] == "reports" && request.HttpMethod == System.Net.Http.HttpMethod.Get.Method)
                {
                    switch (address[1])
                    {
                        case "best-players":
                            statsApi.GetBestPlayersReport(listenerContext);
                            break; 
                        default:
                            statsApi.HandleIncorrect(listenerContext);
                            break;
                    }
                }
                else
                {
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    listenerContext.Response.Close();
                }
            }
            else
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                listenerContext.Response.Close();
            }                                                                        

            //var data_text = new StreamReader(
            //    listenerContext.Request.InputStream,
            //    listenerContext.Request.ContentEncoding).ReadToEnd();

            //var cleaned_data = System.Web.HttpUtility.UrlDecode(data_text);

            //listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
            //using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
            //    writer.WriteLine("Hello, world!");
        }

        private readonly HttpListener listener;
        
        private readonly StatsAPI statsApi;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}