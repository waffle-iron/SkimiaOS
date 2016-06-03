using Nancy;
using Nancy.Owin;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;


namespace SkimiaOS.Plugins.BasePlugin.Modules
{
    public class OWINModule : NancyModule
    {
        public OWINModule()
        {
            Get["/owin-test"] = x => {
                var env = this.Context.GetOwinEnvironment();

                var requestBody = (Stream)env["owin.RequestBody"];
                var requestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];
                var requestMethod = (string)env["owin.RequestMethod"];
                var requestPath = (string)env["owin.RequestPath"];
                var requestPathBase = (string)env["owin.RequestPathBase"];
                var requestProtocol = (string)env["owin.RequestProtocol"];
                var requestQueryString = (string)env["owin.RequestQueryString"];
                var requestScheme = (string)env["owin.RequestScheme"];

                var responseBody = (Stream)env["owin.ResponseBody"];
                var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];

                var owinVersion = (string)env["owin.Version"];
                var cancellationToken = (CancellationToken)env["owin.CallCancelled"];

                var uri = requestScheme + "://" + requestHeaders["Host"].First() +
                  requestPathBase + requestPath;

                if (requestQueryString != "")
                    uri += "?" + requestQueryString;

                var headers = "";
                foreach (var i in requestHeaders)
                {
                    headers += "<h3>"+i.Key.ToString()+"</h3>";
                    foreach(var e in i.Value)
                    {
                        headers += e.ToString() + "<br />";
                    }
                }
                //var y = requestHeaders["dddd"];
                return string.Format("<h1>{0}</h1><h2>Headers:</h2>{3} {1} {2}", requestProtocol, requestMethod, uri, headers);
            };
        }
    }
}
