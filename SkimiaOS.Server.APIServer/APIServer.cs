using Microsoft.Owin.Hosting;
using SkimiaOS.Core.Config;
using SkimiaOS.Server.APIServer.OWIN;
using SkimiaOS.Server.BaseServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Server.APIServer
{
    public class APIServer : ServerBase<APIServer>
    {

        [Configurable]
        public static readonly string Url = "http://+:8080";

        private IDisposable m_host;

        public APIServer() : base(Definitions.ConfigFilePath, Definitions.SchemaFilePath)
        {

        }

        public override void Initialize()
        {
            try
            {
                base.Initialize();

                base.ConsoleInterface = new APIConsole();
                APIConsole.SetTitle("#SkimiaOS API Server");

                this.logger.Info("Register Commands...");
                base.CommandManager.RegisterAll(Assembly.GetExecutingAssembly());

                this.logger.Info("Initialisation...");
                base.InitializationManager.InitializeAll();
                base.IsInitialized = true;

            }
			catch (Exception ex)
			{
				base.HandleCrashException(ex);
				base.Shutdown();
            }

        }

        public override void Start()
        {
            base.Start();
            this.logger.Info("Starting Console Handler Interface...");
            base.ConsoleInterface.Start();
            this.logger.Info("Start listening on: " + APIServer.Url + "...");
            try
            {
                this.m_host = WebApp.Start<Startup>(APIServer.Url);
            }catch(TargetInvocationException e)
            {
                if(e.InnerException != null && typeof( HttpListenerException) == e.InnerException.GetType())
                {
                    logger.Fatal("Unable to Start Http Server Raison : " + e.InnerException.Message);
                    this.Shutdown();
                }
                else
                    this.HandleCrashException(e);
            }
            
            base.StartTime = DateTime.Now;
        }

        protected override void OnShutdown()
        {     
            if(this.m_host != null)  
            this.m_host.Dispose();
            base.OnShutdown();
        }
    }
}
