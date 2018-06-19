using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThinWalls.Models;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace ThinWalls.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            HttpWebRequest WR = WebRequest.CreateHttp("https://api.yelp.com/v3/businesses/search?location=1214+GRISWOLD+ST,+DETROIT,+48226&categories=apartments&sort_by=distance");
            WR.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
            string key = ConfigurationManager.AppSettings["AuthKey"];
            WR.Headers.Add("Authorization", key);

            HttpWebResponse Response;

            try
            {
                Response = (HttpWebResponse)WR.GetResponse();
            }
            catch (WebException e)
            {
                ViewBag.Error = "Exception";
                ViewBag.ErrorDescription = e.Message;
                return View();
            }

            if (Response.StatusCode != HttpStatusCode.OK)
            {
                ViewBag.Error = Response.StatusCode;
                ViewBag.ErrorDescription = Response.StatusDescription;
                return View();
            }

            StreamReader reader = new StreamReader(Response.GetResponseStream());
            string Data = reader.ReadToEnd();

            try
            {

                JObject JsonData = JObject.Parse(Data);
                ViewBag.Data = JsonData["businesses"];



            }
            catch (Exception e)
            {
                ViewBag.Error = "JSON Issue";
                ViewBag.ErrorDescription = e.Message;
                return View();
            }

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
    }
}