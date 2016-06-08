using Nancy;
using Nancy.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Server.APIServer.OWIN
{
    class APIServerBootstrapper : DefaultNancyBootstrapper
    {
        public APIServerBootstrapper()
        {
            ApplicationPipelines.OnError.AddItemToStartOfPipeline((z, a) =>
            {
                APIServer.Instance.HandleCrashException(a);
                return null;
            });

            
        }
        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = @"SkimiaOS" }; }
        }
    }
}
