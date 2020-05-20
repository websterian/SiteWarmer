using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using DocumentFormat.OpenXml.Office.CustomUI;
using Sitecore.Commerce.Engine.Connect.DataProvider;
using Sitecore.Data.Fields;
using Sitecore.Services.Examples.Warmup.Services;
using Sitecore.Shell.Applications.Dialogs.SelectItems;
using Sitecore.Jobs;
using Sitecore.Services.Examples.Warmup.Jobs;
using Sitecore.Services.Examples.Warmup.Models;
using Item = Sitecore.Data.Items.Item;

namespace Sitecore.Services.Examples.Warmup.Controllers
{
    [RoutePrefix("warmup")]
    public class WarmupController : ApiController
    {
        private IWarmupContext _warmupContext;

        public WarmupController(IWarmupContext warmupContext)
        {
            _warmupContext = warmupContext;
        }

        [Route("probe")]
        public IHttpActionResult GetProbe()
        {
            /*
             * A 500 is  thrown if we are busy warming up or just started warming up.
             * This allows the first hit to start the warming and all subsequent hits to see if warming is complete
             * This is because we don't want to exclude the CD from pool just because it wasn't setup.
             */
            if (_warmupContext.WarmedUp)
            {
                var message = "Warmup : The instance is already warm";
                Diagnostics.Log.Info(message, this);
                return this.Ok(message);
            }

            if (_warmupContext.WarmingUp)
            {
                var message = "Warmup : The instance is still warming up";
                Diagnostics.Log.Info(message, this);
                return this.BadRequest(message);
            }

            //Make sure we grab the latest setup
            _warmupContext.PopulateWarmupSettings();

            if (_warmupContext.SiteSettings == null || !_warmupContext.SiteSettings.Any())
            {
                var message = "Warmup : The warm up site settings are not setup";
                Diagnostics.Log.Error(message, this);
                return this.Ok(message);
            }
            
            var service = new WarmupJob(_warmupContext);
            var messageStart = service.StartWarmup();

            //Return a error as its now warming
            return this.BadRequest(messageStart);
        }
    }
}
