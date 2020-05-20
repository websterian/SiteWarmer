using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Sitecore.Services.Examples.Warmup.Services;

namespace Sitecore.Services.Examples.Warmup
{
    public class LargeWebClient : WebClient
    {
        private readonly double _timeout;
        public LargeWebClient(double timeout = 3600)
        {
            _timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            var w = base.GetWebRequest(uri);
            w.Timeout = (int) TimeSpan.FromSeconds(_timeout).TotalMilliseconds;
            return w;
        }
    }
}