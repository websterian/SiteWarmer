using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;

namespace Sitecore.Services.Examples.Warmup.Models
{
    public class WarmupSiteSetting
    {
        public WarmupSiteSetting()
        {
            WarmupItems = new List<Item>();
        }
        public string Sitename { get; set; }
        public string SitemapURL { get; set; }
        public int SitemapDownloadTimeout { get; set; }
        public int PercentageOfSiteToWarmup { get; set; }
        public List<Item> WarmupItems { get; set; }

        public bool VerboseLogging { get; set; }

        public string OnlyVisitUrlsContainsTheFollowingKeywords { get; set; }

        public bool WarmedUp { get; set; }
        public bool WarmingUp { get; set; }

    }
}