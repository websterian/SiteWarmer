using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Services.Examples.Warmup.Models;

namespace Sitecore.Services.Examples.Warmup.Services
{
    public interface IWarmupContext
    {
       bool WarmedUp { get; }

       bool WarmingUp { get; }

       List<WarmupSiteSetting> SiteSettings { get; set; }

       void PopulateWarmupSettings();
    }
}