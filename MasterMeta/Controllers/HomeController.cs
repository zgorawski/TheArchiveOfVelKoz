using MasterMeta.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MasterMeta.Controllers
{
    public class HomeController : Controller
    {
        private static string apiKey = ConfigurationManager.AppSettings["APIKey"];

        IDictionary<string, string> regions = new Dictionary<string, string>
            {
                {"br", "br" },
                {"eune", "eune" },
                {"euw", "euw" },
                {"kr", "kr" },
                {"lan", "lan" },
                {"las", "las" },
                {"na", "na" },
                {"oce", "oce" },
                {"ru", "ru" },
                {"tr", "tr" }
            };

        public ActionResult Index(string error = "")
        {
            var region = getRegionFromCookie();
            ViewBag.Regions = new SelectList(regions, "key", "value", region == null ? "eune" : region);
            ViewBag.ErrorMessage = error;

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult SubmitInputData(InputData inputData)
        {
            if (inputData == null)
            {
                return RedirectToAction("Index", new { error = "Could not find Summoner." });
            }
            else
            {
                HttpContext.Response.SetCookie(new System.Web.HttpCookie("lolRegion", inputData.Region));
            }

            RestClient client = new RestClient("https://" + inputData.Region + ".api.pvp.net");

            // get player ID, based on received input
            var summonerRequest = new RestRequest("/api/lol/" + inputData.Region + "/v1.4/summoner/by-name/" + inputData.SummonerName, Method.GET);
            summonerRequest.AddParameter("api_key", apiKey);

            IRestResponse<Dictionary<string, Summoner>> summonerResponse = client.Execute<Dictionary<string,Summoner>>(summonerRequest);

            if (summonerResponse.Data == null || summonerResponse.StatusCode != HttpStatusCode.OK)
            {
                return RedirectToAction("Index", new { error = "Could not find " + inputData.SummonerName + " from " + inputData.Region + " region" });
            }

            var summonerID = summonerResponse.Data.FirstOrDefault().Value.id;

            return RedirectToAction("Games", new { summonerID = summonerID, region = inputData.Region });
        }
        
        public ActionResult Games(long? summonerID = null, string region = null)
        {
            if (!summonerID.HasValue || String.IsNullOrEmpty(region) || !regions.Values.Contains(region))
            {
                return RedirectToAction("Index", new { error = "invalid data error" });
            }

            RestClient client = new RestClient("https://" + region + ".api.pvp.net");

            // get last matches of given summoner

            var recentGameRequest = new RestRequest("/api/lol/" + region + "/v1.3/game/by-summoner/" + summonerID + "/recent", Method.GET);
            recentGameRequest.AddParameter("api_key", apiKey);

            IRestResponse<Games> recentGameResponse = client.Execute<Games>(recentGameRequest);

            if (recentGameResponse.Data == null || recentGameResponse.StatusCode != HttpStatusCode.OK)
            {
                return RedirectToAction("Index", new { error = "Could not load champion recent games." });
            }

            // get champions static data (for images)

            RestClient globalClient = new RestClient("https://global.api.pvp.net");
            var championsRequest = new RestRequest("/api/lol/static-data/" + region + "/v1.2/champion", Method.GET);
            championsRequest.AddParameter("api_key", apiKey);
            championsRequest.AddParameter("champData", "image");

            IRestResponse<Champions> championsResponse = globalClient.Execute<Champions>(championsRequest);

            if (championsResponse.Data == null || championsResponse.StatusCode != HttpStatusCode.OK)
            {
                return RedirectToAction("Index", new { error = "Could not load champion images." });
            }

            ViewBag.ChampionIdToChampion = championsResponse.Data.ChampionIdToChampion();
            ViewBag.Version = championsResponse.Data.version;
            ViewBag.PlayerId = summonerID;

            return View(recentGameResponse.Data);            
        }

        public ActionResult ItemSet(long? matchId = null, int? teamId = null, int? championId = null)
        {
            if (!matchId.HasValue || !teamId.HasValue || !championId.HasValue)
            {
                return RedirectToAction("Index");
            }

            var region = getRegionFromCookie();
            RestClient client = new RestClient("https://" + region + ".api.pvp.net");
            
            // get match            

            var request = new RestRequest("/api/lol/" + region + "/v2.2/match/" + matchId.Value , Method.GET);
            request.AddParameter("api_key", apiKey);
            request.AddParameter("includeTimeline", "true");
            
            IRestResponse<Match> response = client.Execute<Match>(request);

            if (response.Data == null || response.StatusCode != HttpStatusCode.OK)
            {
                return RedirectToAction("Index", new { error = "Could not load match details." });
            }

            // get participientId (the player which events from timeline should be considered later)

            var match = response.Data;
            var participientId = match.GetParticipientId(championId.Value, teamId.Value);

            // get all events from timeline about buying / selling items

            if (!participientId.HasValue)
            {
                return RedirectToAction("Index", new { error = "Could not find player in match." });
            }

            var items = match.timeline.GetItemsEvents(participientId.Value);
            var itemsSet = new ItemsSet(items);
            string fileName = "itemsSet.json";

            // get name of champion (for better file name)

            RestClient globalClient = new RestClient("https://global.api.pvp.net");
            var championsRequest = new RestRequest("/api/lol/static-data/" + region + "/v1.2/champion/" + championId.Value, Method.GET);
            championsRequest.AddParameter("api_key", apiKey);

            IRestResponse<Champion> championResponse = globalClient.Execute<Champion>(championsRequest);

            if (championResponse.Data != null && championResponse.StatusCode == HttpStatusCode.OK)
            {
                itemsSet.title = championResponse.Data.name + " - Vel'Koz Archive";
                fileName = championResponse.Data.name + "ItemsSet.json";
            }

            var contentType = "application/json";
            var content = JsonConvert.SerializeObject(itemsSet);
            var bytes = Encoding.UTF8.GetBytes(content);
            var result = new FileContentResult(bytes, contentType);
            result.FileDownloadName = fileName;
            return result;
        }        

        /// <summary>
        /// Gets game version, from cookie, or from API request
        /// </summary>
        /// <returns>Game version or fallbacks to "5.16.1"</returns>
        private string getGameVersion(string region)
        {
            string lolVersion = "5.16.1";
            var lolVersionCookie = Request.Cookies["lolVersion"];

            RestClient client = new RestClient("https://global.api.pvp.net");

            if (lolVersionCookie == null)
            {
                var request = new RestRequest("/api/lol/static-data/" + region + "/v1.2/versions", Method.GET);
                request.AddParameter("api_key", apiKey);

                IRestResponse<List<string>> response = client.Execute<List<string>>(request);

                if (response.Data != null && response.StatusCode == HttpStatusCode.OK)
                {
                    lolVersion = response.Data.FirstOrDefault();
                    HttpContext.Response.SetCookie(new System.Web.HttpCookie("lolVersion", lolVersion));
                }

            } else
            {
                lolVersion = lolVersionCookie.Value;
            }
            
            return lolVersion;
        }

        /// <summary>
        /// Gets region from cookie or returns null
        /// </summary>
        private string getRegionFromCookie()
        {
            string result = null;
            var lolRegionCookie = Request.Cookies["lolRegion"];

            if (lolRegionCookie != null && !String.IsNullOrEmpty(lolRegionCookie.Value))
            {
                result = lolRegionCookie.Value;
            }
            
            return result;
        }
    }
}