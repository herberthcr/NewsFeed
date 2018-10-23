using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml.Linq;
using NewsFeed.Models;

namespace NewsFeed.Controllers
{
    public class HomeController : Controller
    {
        #region Index
        /// <summary>
        /// Initial method, returns subscribed news feed from database
        /// </summary>
        /// <returns>RssFeed List</returns>
        public ActionResult Index()
        {
            List<RssFeed> items = new List<RssFeed>();
            using (NewsFeedDBEntities dc = new NewsFeedDBEntities())
            {
                //Gets subscribed news feed from DB
                var c = dc.News.OrderByDescending(a => a.NewsId).ToList();
                foreach (var i in c)
                {
                    RssFeed item = new RssFeed
                    {
                        Title = i.Title,
                        Description = i.Content,
                        PubDate = i.Date.ToShortDateString(),
                        Image = (i.Image != null && i.Image != "") ? i.Image : "~/Content/img/no_image.jpg",
                        Link = i.Url,
                        RSSUrl = i.RSSUrl
                    };
                    items.Add(item);
                }
            }
            ViewBag.RSSFeed = items;
            return View();
        }
        /// <summary>
        /// ///  Creates a new subscription to a news feed
        /// </summary>
        /// <param name="RSSURL"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(string RSSURL)
        {
            try
            {
                if (RSSURL != null && RSSURL != "" && NewsFeedExist(RSSURL))
                {
                    using (NewsFeedDBEntities dc = new NewsFeedDBEntities())
                    {
                        // Get Uri Syndicate Info
                        var reader = System.Xml.XmlReader.Create(RSSURL);
                        var feed = SyndicationFeed.Load(reader);

                        var newsFeed = new News
                        {
                            Title = feed.Title != null ? feed.Title.Text : "",
                            Url = feed.Links != null ? feed.Links.FirstOrDefault().Uri.AbsoluteUri : "",
                            Content = feed.Description != null ? feed.Description.Text : "",
                            Date = (feed.LastUpdatedTime != null && feed.LastUpdatedTime.DateTime != DateTime.MinValue) ? feed.LastUpdatedTime.DateTime : DateTime.Now,
                            Image = feed.ImageUrl != null ? feed.ImageUrl.AbsoluteUri : "~/Content/img/no_image.jpg",
                            RSSUrl = RSSURL
                        };

                        //Create new feed
                        if (newsFeed != null)
                        {
                            dc.News.Add(newsFeed);
                            dc.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }

            List<RssFeed> items = new List<RssFeed>();
            using (NewsFeedDBEntities dc = new NewsFeedDBEntities())
            {
                //Gets subscribed news feed from DB
                var c = dc.News.OrderByDescending(a => a.NewsId).ToList();
                foreach (var i in c)
                {
                    RssFeed item = new RssFeed
                    {
                        Title = i.Title,
                        Description = i.Content,
                        PubDate = i.Date.ToShortDateString(),
                        Image = i.Image,
                        Link = i.Url
                    };
                    items.Add(item);
                }
            }
            ViewBag.RSSFeed = items;
            return View();
        }
        #endregion

        #region NewsItems
        /// <summary>
        /// Gets all news items from all subscriptions, or an specific subscription
        /// </summary>
        /// <param name="rssOutlet">news feed outlet</param>
        /// <returns></returns>
        public ActionResult NewsItems(string rssOutlet = "")
        {
            List<SyndicationItem> items = new List<SyndicationItem>();
            using (NewsFeedDBEntities dc = new NewsFeedDBEntities())
            {
                List<News> newsList = new List<News>();
                if(rssOutlet != null && rssOutlet != "") { 
                    newsList = dc.News.Where(n => n.RSSUrl == rssOutlet).OrderByDescending(a => a.NewsId).ToList();
                }
                else
                {
                    newsList = dc.News.OrderByDescending(a => a.NewsId).ToList();
                }

                foreach (var i in newsList)
                {
                    var reader = System.Xml.XmlReader.Create(i.RSSUrl);
                    var feed = SyndicationFeed.Load(reader);
                    items.AddRange(feed.Items);
                }
            }
            items = items.OrderByDescending(a => a.PublishDate).ToList();
            return View(items);
        }

        /// <summary>
        /// Get items depending on search keyword
        /// </summary>
        /// <param name="rssOutlet">news feed outlet</param>
        /// <param name="keyword">keyword used for search</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult NewsItems(string rssOutlet = "", string keyword = "")
        {
            List<SyndicationItem> items = new List<SyndicationItem>();
            using (NewsFeedDBEntities dc = new NewsFeedDBEntities())
            {
                List<News> newsList = new List<News>();
                if (rssOutlet != null && rssOutlet != "")
                {
                    newsList = dc.News.Where(n => n.RSSUrl == rssOutlet).OrderByDescending(a => a.NewsId).ToList();
                }
                else
                {
                    newsList = dc.News.OrderByDescending(a => a.NewsId).ToList();
                }
                
                foreach (var i in newsList)
                {
                    var reader = System.Xml.XmlReader.Create(i.RSSUrl);
                    var feed = SyndicationFeed.Load(reader);
                    if (keyword != "")
                    {
                        feed.Items = feed.Items.Where(n => n.Title.Text.ToLower().Contains(keyword.ToLower())).ToList();
                    }
                    items.AddRange(feed.Items);
                }
            }
            items = items.OrderByDescending(a => a.PublishDate).ToList();
            return View(items);
        }
#endregion

        public ActionResult About()
        {
            ViewBag.Message = "Herberth's Information.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Herberth contact page.";
            return View();
        }

        /// <summary>
        /// If the news outlet is not already on the database returns true
        /// </summary>
        /// <param name="RSSURL"></param>
        /// <returns></returns>
        private bool NewsFeedExist(string RSSURL) {
            using (NewsFeedDBEntities dc = new NewsFeedDBEntities())
            {
                var rssNews = dc.News.Where(a => a.RSSUrl == RSSURL).OrderByDescending(a => a.NewsId).ToList();
                if (rssNews.Count > 0)
                    return false;
            }
            return true;
        }


    }
}