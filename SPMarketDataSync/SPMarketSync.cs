using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Plumsail.SPMarketDataSync.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using App = Plumsail.SPMarketDataSync.Models.App;

namespace Plumsail.SPMarketDataSync
{
    public sealed class SPMarketSyncManager : IDisposable
    {
        #region Declarations
        private DateTime _now = DateTime.Now;
        private SPMarketDBContext _dbContext = new SPMarketDBContext();
        private CookieContainer _cookieContainer;

        private string _siteUrl;

        const string BasicPath     = "/_layouts/15/storefront.aspx?";
        const string AppCategories = BasicPath + "task=GetCategories&catalog=0";
        const string AllApps       = BasicPath + "task=GetApps&catalog=0&category={0}";
        const string AppDetails    = BasicPath + "task=GetAppDetails&catalog=0&appid={0}";
        #endregion

        public SPMarketSyncManager(string siteUrl, string login, string password)
        {
            var creds = new SharePointOnlineCredentials(login, password);
            var auth  = creds.AuthenticateAsync(new Uri(siteUrl), true);

            _siteUrl = siteUrl;
            _cookieContainer = auth.Result.CookieContainer;
        }

        public void Sync()
        {
            var dbApps  = _dbContext.Apps.ToList(); //pre load data
            var appsIds = GetAppsIds();

            while (appsIds.Count > 0)
            {
                try
                {
                    var index   = appsIds.Count - 1;
                    var assetId = appsIds[index] as string;

                    Console.Write("\r Total {0} ", index);

                    var app   = GetApp(assetId);
                    var dbApp = dbApps.FirstOrDefault(item => item.AssetId == assetId);

                    if (dbApp == null)
                        dbApp = _dbContext.Apps.Add(app);
                    else 
                        dbApp.HistoricalData.Add(app.HistoricalData.First());

                    appsIds.RemoveAt(index);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }

            _dbContext.SaveChanges();
        }

        #region Private business methods
        /// <summary>
        /// Get dictionary of apps from all catchegories
        /// </summary>
        /// <returns></returns>
        private OrderedDictionary GetAppsIds()
        {
            var result     = new OrderedDictionary();
            var categories = ExecuteRequestGet(_siteUrl + AppCategories);

            foreach (var cathegory in categories)
            {
                var AppsInCath = string.Format(AllApps, cathegory.ID);
                dynamic apps   = ExecuteRequestGet(_siteUrl + AppsInCath);
                foreach (var app in apps)
                {
                    var assetId = app.AssetId.ToString();
                    result[assetId] = assetId;
                }
            }

            return result;
        }

        /// <summary>
        /// Get details app information by assetID
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        private App GetApp(string assetId)
        {
            var appDetailsUrl = string.Format(AppDetails, assetId);

            dynamic main = ExecuteRequestGet(_siteUrl + appDetailsUrl);
            var details  = main.BasicDetails;
            var app      = new App()
            {
                AssetId            = details.AssetId
                , Title            = details.Title
                , ShortDescription = details.ShortDescription
                , ThumbnailUrl     = details.ThumbnailUrl
                , Publisher        = details.Publisher
                , Price            = details.Price
                , CategoryID       = details.CategoryID
                , PriceType        = details.PriceType
                , Description      = main.Description
                , PriceValue       = main.PriceValue
                , PublisherUrl     = main.PublisherUrl
                , ReleasedDate     = DateTime.Parse(main.ReleasedDate.ToString())
            };
            
            app.HistoricalData.Add(new History()
            {
                AssetId     = details.AssetId
                , Date      = _now
                , Downloads = main.Downloads
                , Votes     = details.Votes
                , Rating    = details.Rating
            });

            return app;
        }
        #endregion

        #region Private utilty methods
        private dynamic ExecuteRequestGet(string url)
        {
            var response        = ExecuteRequest(new Uri(url), "GET", null);
            var responseString  = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return JsonConvert.DeserializeObject(responseString);
        }

        private HttpWebResponse ExecuteRequest(Uri url, string method, Dictionary<string, string> data)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method            = method;
            request.AllowAutoRedirect = false;
            request.Timeout           = 60*1000;
            request.CookieContainer   = _cookieContainer;

            if (data != null && data.Count > 0)
            {
                var inputFields = new List<string>();
                foreach (var key in data.Keys)
                {
                    inputFields.Add(string.Format("{0}={1}", key, PostSafe(data[key])));
                }
                var streamData = Encoding.ASCII.GetBytes(string.Join("&", inputFields.ToArray()));

                request.ContentLength = streamData.Length;
                request.ContentType   = "application/x-www-form-urlencoded";

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(streamData, 0, streamData.Length);
                }
            }

            return (HttpWebResponse)request.GetResponse();
        }

        private static string PostSafe(string value)
        {
            return WebUtility.UrlEncode(WebUtility.HtmlDecode(value));
        }
        #endregion

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
