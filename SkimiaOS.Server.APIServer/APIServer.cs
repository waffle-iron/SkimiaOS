using Microsoft.Owin.Hosting;
using NLog;
using SkimiaOS.Core.Config;
using SkimiaOS.Core.Messages;
using SkimiaOS.Server.APIServer.Messages;
using SkimiaOS.Server.APIServer.OWIN;
using SkimiaOS.Server.BaseServer;
using SkimiaOS.Server.BaseServer.Plugins;
using SkimiaOS.Server.BaseServer.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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

            if(!Debugger.IsAttached)
            {
                try
                {
                    base.Initialize();
                    this.init();
                

                }
			    catch (Exception ex)
			    {
				    base.HandleCrashException(ex);
				    base.Shutdown();
                }
            }else
            {
                base.Initialize();
                this.init();
            }
            

        }

        private void init()
        {
            base.ConsoleInterface = new APIConsole();
            APIConsole.SetTitle("#SkimiaOS API Server");

            this.logger.Info("Register Commands...");
            base.CommandManager.RegisterAll(Assembly.GetExecutingAssembly());

            this.logger.Info("Initialisation...");
            base.InitializationManager.InitializeAll();
            base.IsInitialized = true;

            var msg = new APIServerInitializationMessage();
            DispatcherTask.Dispatcher.Enqueue(msg, this);

            msg.Wait();
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

        [MessageHandler(typeof(PluginRemovedMessage))]
        public static void HandlePluginRemovedMessage(object sender, PluginRemovedMessage message)
        {
            LogManager.GetCurrentClassLogger().Info("HOOKPlugin Unloaded : {0}", message.PluginContext.Plugin.GetDefaultDescription());

        }
        [MessageHandler(typeof(PluginAddedMessage))]
        public static void HandlePluginAddedMessage(object sender, PluginAddedMessage message)
        {
            LogManager.GetCurrentClassLogger().Info("HOOKPlugin Loaded : {0}", message.PluginContext.Plugin.GetDefaultDescription());
        }
    }
}
