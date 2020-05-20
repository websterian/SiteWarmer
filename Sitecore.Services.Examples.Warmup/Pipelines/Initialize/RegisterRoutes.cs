using System.Web.Routing;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;

namespace Sitecore.Services.Examples.Warmup.Pipelines.Initialize
{
    public class RegisterRoutes
    {
        public void Process(PipelineArgs args)
        {
            this.RegisterHttpRoutes(RouteTable.Routes);
        }

        private void RegisterHttpRoutes(RouteCollection routeCollection)
        {
            Assert.ArgumentNotNull(routeCollection, nameof(routeCollection));
        }
    }
}
