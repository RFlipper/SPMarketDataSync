using System.Configuration;

namespace Plumsail.SPMarketDataSync
{
    class Program
    {
        private static void Main(string[] args)
        {
            var url      = ConfigurationManager.AppSettings["Url"];
            var login    = ConfigurationManager.AppSettings["Login"];
            var password = ConfigurationManager.AppSettings["Password"];

            using (var marketManager = new SPMarketSyncManager(url, login, password))
            {
                marketManager.Sync();
            }
        }
    }
}
