using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsFeed.Models
{
    public class RssFeed
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string RSSUrl { get; set; }
        public string Description { get; set; }
        public string PubDate { get; set; }
        public string Image { get; set; }
    }
}