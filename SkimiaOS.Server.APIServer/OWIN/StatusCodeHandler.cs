using Nancy;
using Nancy.ErrorHandling;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Server.APIServer.OWIN
{
    public class StatusCodeHandler : IStatusCodeHandler
    {
        private static IEnumerable<int> _checks = new List<int>()
        {
            404,
            500
        };

        public static IEnumerable<int> Checks { get { return _checks; } }

       

        public StatusCodeHandler()
        {
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            if ((_checks.Any(x => x == (int)statusCode)))
                this.Handle(statusCode, context);

            return false ;
        }


        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            Logger logger = LogManager.GetCurrentClassLogger();

            logger.Warn((int)statusCode +" "+ statusCode.ToString());
        }
    }
}
