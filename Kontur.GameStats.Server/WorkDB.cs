using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server
{
    /// <summary>
    /// Работа с БД
    /// </summary>
    public static class WorkDB
    {
        private static string connectionString = "";

        public static bool Connect()
        {
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                SqlConnection dbConnection = new SqlConnection(connectionString);

                dbConnection.Open();
                dbConnection.Close();
                dbConnection.Dispose();

                return true;
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, error.Message));
                Console.ResetColor();
            }

            return false;
        }


    }
}
