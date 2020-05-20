using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml;
using Sitecore.Data.Items;
using Sitecore.Jobs;
using Sitecore.Links;
using Sitecore.Links.UrlBuilders;
using Sitecore.Services.Examples.Warmup.Models;
using Sitecore.Services.Examples.Warmup.Services;
using Sitecore.Sites;

namespace Sitecore.Services.Examples.Warmup.Jobs
{
    public class WarmupJob
    {
        private const string JobName = "WarmupJob";

        private readonly IWarmupContext _context;
        
        public WarmupJob(IWarmupContext context)
        {
            _context = context;
        }

        public Sitecore.Abstractions.BaseJob Job => JobManager.GetJob(JobName);

        private string StartWarmupSiteJob(WarmupSiteSetting siteSetting)
        {
            siteSetting.WarmingUp = true;
            siteSetting.WarmedUp = false;

            var options = new DefaultJobOptions(JobName + " - " + siteSetting.Sitename,
                JobName,
                siteSetting.Sitename,
                this,
                "WarmupSiteStart",
                new object[] { siteSetting });

            JobManager.Start(options);
            return JobName;
        }

        public string StartWarmup()
        {
            foreach (var siteSetting in _context.SiteSettings)
            {
                StartWarmupSiteJob(siteSetting);
            }

            var message = $"Started at {DateTime.Now}";
            LogMessage(message, null);
            return message;
        }

        private void WarmupSiteStart(WarmupSiteSetting siteSetting)
        {
            try
            {
                //Are we warming up any specific items
                if (siteSetting.WarmupItems.Any())
                {
                    var siteInfo = Sitecore.Configuration.Factory.GetSiteInfoList()
                        .FirstOrDefault(x =>
                            string.Equals(x.Name, siteSetting.Sitename, StringComparison.CurrentCultureIgnoreCase));

                    if (siteInfo != null)
                    {
                        var siteContext = Sitecore.Configuration.Factory.GetSite(siteInfo.Name);

                        using (new SiteContextSwitcher(siteContext))
                        {
                            WarmupItems(siteSetting.WarmupItems, siteSetting);
                        }
                    }
                }

                /*
                 * Credit to Elena Mosoff for sitemap processing idea :)
                 */

                //Are we warming up by Site map
                if (!string.IsNullOrEmpty(siteSetting.SitemapURL))
                    WarmupSitemap(siteSetting);
            }
            catch (Exception e)
            {
                //TODO : If there is a error we mark the CD as warm, perhaps this should be an option
                LogMessage($"Could not finish warming job {e.Message}", siteSetting);
            }
            finally
            {
                siteSetting.WarmingUp = false;
                siteSetting.WarmedUp = true;
                LogMessage($"Finished at {DateTime.Now}", siteSetting);
            }
        }

        private void WarmupSitemap(WarmupSiteSetting siteSetting)
        {
            var baseUrl = siteSetting.SitemapURL;
            var wc = new LargeWebClient(siteSetting.SitemapDownloadTimeout) { Encoding = System.Text.Encoding.UTF8 };

            LogMessage($"Download site map started at {DateTime.Now}", siteSetting);
            var reply = wc.DownloadString(baseUrl);
            LogMessage($"Download site map ended at {DateTime.Now}", siteSetting);

            var urlDoc = new XmlDocument();
            urlDoc.LoadXml(reply);

            var xnList = urlDoc.GetElementsByTagName("url");

            var totalNodes = xnList.Count;
            var nodesToProcess = (totalNodes * siteSetting.PercentageOfSiteToWarmup) / 100;
            var nodesProcessed = 0;

            LogMessage($"Starting to warm up using site map, {siteSetting.PercentageOfSiteToWarmup}% of {totalNodes} nodes equals {nodesToProcess}, started at {DateTime.Now}", siteSetting);

            foreach (XmlNode node in xnList)
            {
                if (nodesProcessed >= nodesToProcess)
                    break;

                var client = new HttpClient();
                var url = node["loc"]?.InnerText;

                if (!CheckKeyword(siteSetting, url))
                {
                    if (siteSetting.VerboseLogging)
                        LogMessage($"URL {url} does not contain any of the keywords, skipped", siteSetting);

                    continue;
                }

                if(siteSetting.VerboseLogging)
                    LogMessage($"Visiting URL {url} started at {DateTime.Now}", siteSetting);

                try
                {
                    var content = wc.DownloadString(url);
                }
                catch (Exception e)
                {
                    LogMessage($"Could not visit {url} error {e.Message}", siteSetting);
                }

                if (siteSetting.VerboseLogging)
                    LogMessage($"Visiting URL {url} completed at {DateTime.Now}", siteSetting);

                nodesProcessed++;
            }

            LogMessage($"Ending warm up using site map started at {DateTime.Now}", siteSetting);
        }

        private static bool CheckKeyword(WarmupSiteSetting siteSetting, string url)
        {
            if (string.IsNullOrEmpty(siteSetting.OnlyVisitUrlsContainsTheFollowingKeywords))
            {
                return true;
            }

            var keywords = siteSetting?.OnlyVisitUrlsContainsTheFollowingKeywords?.Split(new char[] {'|'});

            var containsKeyword = keywords.Any(url.Contains);

            return containsKeyword;
        }

        private void LogMessage(string message, WarmupSiteSetting siteSetting)
        {
            //TODO : use setting from site
            if(siteSetting == null)
                Diagnostics.Log.Info("Warmup : " + message, this);
            else
            {
                Diagnostics.Log.Info($"Warmup {siteSetting.Sitename} : " + message, this);
            }
        }

        private void WarmupItems(IEnumerable<Item> items, WarmupSiteSetting siteSetting)
        {
            foreach (var item in items)
            {
                var probe = item.DisplayName;

                if (siteSetting.VerboseLogging)
                    LogMessage($"Item {item.Paths.FullPath} retrieved", siteSetting);

                if (!item.HasChildren) 
                    continue;

                var childrenItems = item.GetChildren()
                    .Cast<Item>().ToArray();

                WarmupItems(childrenItems, siteSetting);
            }
        }
    }
}