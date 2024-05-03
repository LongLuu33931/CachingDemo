using eSMSDemo.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using StackExchange.Redis;
using System.Text;
using Newtonsoft.Json;
using Enyim.Caching.Configuration;
using Enyim.Caching;
using Enyim.Caching.Memcached;

namespace eSMSDemo.Controllers
{
    public class HomeController : Controller
    {
        private string ApiKey = "CBA0136C0E3C5244BE55580144B055";
        private string SecretKey = "0C2058ACEC6F2BDDE071172DD7ACD6";
        private string baseUrl = "http://rest.esms.vn/MainService.svc/json";
        private ConnectionMultiplexer _redis;
        private MemcachedClientConfiguration _memCacheConfig = new MemcachedClientConfiguration();
        private IMemcachedClient _memCacheClient;
        private IDatabase _db;
        public HomeController()
        {
            _redis = ConnectionMultiplexer.Connect("localhost");
            _db = _redis.GetDatabase();
            _memCacheConfig.AddServer("localhost", 11211);
            _memCacheClient = new MemcachedClient(_memCacheConfig);
        }
        //private Memca
        //public HomeController()
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public async Task<ActionResult> SendSMS(string phone, string content)
        {
            string url = $"{baseUrl}/{Constants.Constants.Urls.SendSMSMethodGetType2}?Phone={phone}&Content={content}&ApiKey={ApiKey}&SecretKey={SecretKey}&IsUnicode=0&SmsType=2&Sandbox=1&Brandname=Baotrixemay";

            string result = await SendRequestAsync(url);
            TempData["Result"] = result;
            return RedirectToAction("Index");
        }
        public async Task<string> SendRequestAsync(string url)
        {
            string result = "";
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch
            {

            }
            return result;
        }
        public ActionResult Demo()
        {
            return View();
        }

        public ActionResult GetData()
        {
                List<DemoModel> data = new List<DemoModel>();
            try
            {
                string keyCache = HttpContext.Request.Path;
                string cacheData = _db.StringGet(keyCache);
                if (!string.IsNullOrEmpty(cacheData))
                {
                    data = JsonConvert.DeserializeObject<List<DemoModel>>(cacheData);
                    foreach(var item in data)
                    {
                        item.Caching = "Redis";
                    }
                }
                else
                {
                    data = new List<DemoModel>() {
                    new DemoModel { OrderID= 1, OrderDate= "2017-11-06T12:00:00", Freight= 12.34, ShipCity = "Antwerp", ShipCountry= "Belgium"},
                    new DemoModel { OrderID= 2, OrderDate= "2019-03-02T12:00:00", Freight= 23.45, ShipCity= "Singapore", ShipCountry= "Singapore"},
                    new DemoModel { OrderID= 3, OrderDate= "2019-06-26T12:00:00", Freight= 34.56, ShipCity= "Shanghai", ShipCountry= "China"}
                };
                    var stringData = JsonConvert.SerializeObject(data);
                    _db.StringSet(keyCache, stringData, TimeSpan.FromMinutes(10));
                }
                _redis.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
                return Json(data, JsonRequestBehavior.AllowGet);
            
        }
        public ActionResult GetDataWithMemCached()
        {
                List<DemoModel> data = new List<DemoModel>();
            try
            {
                string keyCache = HttpContext.Request.Path;
                string cachedData = _memCacheClient.Get<string>(keyCache);
                if(cachedData != null)
                {
                    data = JsonConvert.DeserializeObject<List<DemoModel>>(cachedData);
                    foreach(var item in data)
                    {
                        item.Caching = "MemCached";
                    }
                }
                if(data.Count == 0)
                {
                    data = new List<DemoModel>() {
                    new DemoModel { OrderID= 1, OrderDate= "2017-11-06T12:00:00", Freight= 12.34, ShipCity = "Antwerp", ShipCountry= "Belgium"},
                    new DemoModel { OrderID= 2, OrderDate= "2019-03-02T12:00:00", Freight= 23.45, ShipCity= "Singapore", ShipCountry= "Singapore"},
                    new DemoModel { OrderID= 3, OrderDate= "2019-06-26T12:00:00", Freight= 34.56, ShipCity= "Shanghai", ShipCountry= "China"}
                    };
                    var json = JsonConvert.SerializeObject(data);
                    var result = _memCacheClient.Store(StoreMode.Set, keyCache, json);
                }
            }
            catch(Exception ex)
            {
                _memCacheClient.FlushAll();
                Console.WriteLine(ex.Message);
            }
                return Json(data, JsonRequestBehavior.AllowGet);
        }
        public static IMemcachedClient InitializeMemCachedClient()
        {
            MemcachedClientConfiguration config = new MemcachedClientConfiguration();
            config.AddServer("127.0.0.2", 11212);
            MemcachedClient client = new MemcachedClient(config);
            return client;
        }
    }
}