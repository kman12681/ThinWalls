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
using System.Collections;

namespace ThinWalls.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()

        {

            return View();
        }


        public ActionResult Search(string name = "", string location = "", int zipcode = 48226, string category = "all", int radius = 10000)
        {

            HttpWebRequest WR = WebRequest.CreateHttp($"https://api.yelp.com/v3/businesses/search?term={name}&location={location},{zipcode}&categories={category}&radius={radius}&sort_by=distance&limit=50");
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
                return View("Results");
            }

            if (Response.StatusCode != HttpStatusCode.OK)
            {
                ViewBag.Error = Response.StatusCode;
                ViewBag.ErrorDescription = Response.StatusDescription;
                return View("Results");
            }

            StreamReader reader = new StreamReader(Response.GetResponseStream());
            string Data = reader.ReadToEnd();

            try
            {
                JObject JsonData = JObject.Parse(Data);
                ViewBag.Data = JsonData["businesses"];

                //Logic for displaying score on results page

                ThinWallsEntities db = new ThinWallsEntities(); //pulls info from db  
                Dictionary<string, int> scores = new Dictionary<string, int>(); //Dictionary to hold id and average
                List<Review> reviews = db.Reviews.ToList();
                List<int> scoreList = new List<int>();
                int counter = 1;
                List<Review> count = new List<Review>();


                for (int i = 0; i < reviews.Count; i++)
                {
                    for (int j = 0; j < ViewBag.Data.Count; j++)
                    {
                        var id = JsonData["businesses"][j]["id"];

                        if (reviews[i].YelpID == (string)id)
                        {
                            if (!scores.ContainsKey(reviews[i].YelpID))
                            {
                                scores.Add(reviews[i].YelpID, reviews[i].WallScore);
                            }
                            else
                            {
                                count = (from s in db.Reviews
                                         where s.YelpID == ((string)id)
                                         select s).ToList();
                                counter++;
                                scores[reviews[i].YelpID] += reviews[i].WallScore;
                            }
                        }
                        //if (counter > 1)
                        //{
                        //    scores[reviews[i].YelpID] = (scores[reviews[i].YelpID] / counter);
                        //}
                    }

                }
                ViewBag.Scores = scores;

            }
            catch (Exception e)
            {
                ViewBag.Error = "JSON Issue";


                ViewBag.ErrorDescription = e.Message;


                return View("Results");
            }

            return View("Results");

        }


        public ActionResult Details(string id)
        {
            HttpWebRequest WR = WebRequest.CreateHttp($"https://api.yelp.com/v3/businesses/{id}");
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
                ViewBag.Data = JsonData;

            }
            catch (Exception e)
            {
                ViewBag.Error = "JSON Issue";
                ViewBag.ErrorDescription = e.Message;
                return View();
            }

            //Calculate Wall Score Logic
            ThinWallsEntities db = new ThinWallsEntities(); //pulls info from db

            List<Review> reviews = (from r in db.Reviews
                                    where r.YelpID == id
                                    select r).ToList(); //pulls only item w/ matching yelpid
            ViewBag.Reviews = reviews;
            int average = 0;
            foreach (var r in reviews)
            {
                average += r.WallScore;
            }
            if (average == 0)
            {
                ViewBag.Score = average;
            }
            else
            {
                average = average / reviews.Count; //calculates average of all the scores
                ViewBag.Score = average; //so view can access the info           

            }

            HttpWebRequest WR2 = WebRequest.CreateHttp($"https://api.yelp.com/v3/businesses/{id}/reviews");
            WR2.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
            //string key2 = ConfigurationManager.AppSettings["AuthKey"];
            WR2.Headers.Add("Authorization", key);
            HttpWebResponse Response2;

            try
            {
                Response2 = (HttpWebResponse)WR2.GetResponse();
            }
            catch (WebException e)
            {
                ViewBag.Error = "Exception";
                ViewBag.ErrorDescription = e.Message;
                return View();
            }

            if (Response.StatusCode != HttpStatusCode.OK)
            {
                ViewBag.Error = Response2.StatusCode;
                ViewBag.ErrorDescription = Response2.StatusDescription;
                return View();
            }

            StreamReader reader2 = new StreamReader(Response2.GetResponseStream());
            string yelpReviews = reader2.ReadToEnd();

            try
            {
                JObject JsonData2 = JObject.Parse(yelpReviews);
                ViewBag.YelpReviews = JsonData2["reviews"];

            }
            catch (Exception e)
            {
                ViewBag.Error = "JSON Issue";
                ViewBag.ErrorDescription = e.Message;
                return View();
            }

            return View();
        }


    }
}