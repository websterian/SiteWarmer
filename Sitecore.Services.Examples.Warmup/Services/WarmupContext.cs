using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Engine.Connect.DataProvider;
using Sitecore.Services.Examples.Warmup.Models;

namespace Sitecore.Services.Examples.Warmup.Services
{
    public class WarmupContext : IWarmupContext
    {
        private const string SitecoreSystemSettingsWarmupWarmupSettings = "/sitecore/system/Settings/Warmup/Warmup Settings";
        private const string FieldName = "Sitename";
        private const string SitemapDownloadTimeout = "Sitemap download timeout";
        private const string PercentageOfSiteToWarmUp = "Percentage of site to warm up";
        private const string OnlyVisitUrlsContainsTheFollowingKeywords = "Only visit URLs contains the following keywords";
        private const string VerboseLogging = "Verbose logging";
        private const string SitemapUrl = "Sitemap URL";
        private const string WarmupItems = "Warmup items";
        private const string WarmupThereWasAnErrorWhileRetrievingTheWarmupSettings = "Warmup : There was an error while retrieving the warmup settings ";

        public WarmupContext()
        {
            SiteSettings = new List<WarmupSiteSetting>();
        }

        public bool WarmedUp => SiteSettings.Any() && SiteSettings.All(x => x.WarmedUp);

        public bool WarmingUp => SiteSettings.Any() && SiteSettings.Any(x => x.WarmingUp);

        public List<WarmupSiteSetting> SiteSettings { get; set; }

        public void PopulateWarmupSettings()
        {
            this.SiteSettings = new List<WarmupSiteSetting>();
            try
            {
                var warmupSettingsFolderItem = Sitecore.Context.Database.GetItem(SitecoreSystemSettingsWarmupWarmupSettings);
                foreach (var siteSettingItem in warmupSettingsFolderItem.GetChildren()
                    .Cast<Data.Items.Item>().ToArray())
                {
                    var siteSetting = new WarmupSiteSetting
                    {
                        Sitename = siteSettingItem.Fields[FieldName]?.Value.ToString(),
                        SitemapDownloadTimeout = string.IsNullOrEmpty(siteSettingItem.Fields[SitemapDownloadTimeout]?.Value) ? 3600 : System.Convert.ToInt32(siteSettingItem.Fields[SitemapDownloadTimeout]?.Value),
                        PercentageOfSiteToWarmup = string.IsNullOrEmpty(siteSettingItem.Fields[PercentageOfSiteToWarmUp]?.Value) ? 100 : System.Convert.ToInt32(siteSettingItem.Fields[PercentageOfSiteToWarmUp]?.Value),
                        SitemapURL = siteSettingItem.Fields[SitemapUrl]?.Value.ToString(),
                        OnlyVisitUrlsContainsTheFollowingKeywords = siteSettingItem.Fields[OnlyVisitUrlsContainsTheFollowingKeywords]?.Value.ToString(),
                        VerboseLogging = ((Data.Fields.CheckboxField) siteSettingItem.Fields[VerboseLogging]).Checked
                    };

                    Data.Fields.MultilistField warmupItemsField = siteSettingItem.Fields[WarmupItems];
                    siteSetting.WarmupItems = warmupItemsField.GetItems().ToList();

                    this.SiteSettings.Add(siteSetting);
                }
            }
            catch (Exception e)
            {
                var message = WarmupThereWasAnErrorWhileRetrievingTheWarmupSettings + e.Message;
                Diagnostics.Log.Error(message, this);
            }
        }
    }
}