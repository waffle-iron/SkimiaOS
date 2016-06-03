using SkimiaOS.Core.Config;
using SkimiaOS.Core.Reflection;
using SkimiaOS.Server.BaseServer.Commands;
using SkimiaOS.Server.BaseServer.Initialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Server.BaseServer.Plugins
{
    public abstract class PluginBase : IPlugin
    {
        public PluginContext Context
        {
            get;
            protected set;
        }
        public virtual bool UseConfig
        {
            get
            {
                return false;
            }
        }
        public virtual bool AllowConfigUpdate
        {
            get
            {
                return this.UseConfig;
            }
        }
        public virtual string ConfigFileName
        {
            get
            {
                return null;
            }
        }
        public Config Config
        {
            get;
            protected set;
        }
        public abstract string Name
        {
            get;
        }
        public abstract string Description
        {
            get;
        }
        public abstract string Author
        {
            get;
        }
        public abstract Version Version
        {
            get;
        }
        protected PluginBase(PluginContext context)
        {
            this.Context = context;
        }
        public virtual void Initialize()
        {
            //place l'initialisation de lassembly dans tout les managers

            Singleton<InitializationManager>.Instance.AddAssembly(this.Context.PluginAssembly);
            if (ServerBase.InstanceAsBase.IsInitialized)
            {
                ServerBase.InstanceAsBase.IOTaskPool.AddMessage(new Action(Singleton<InitializationManager>.Instance.InitializeAll));
            }

            //register contained commands
            Singleton<CommandManager>.Instance.RegisterAll(this.Context.PluginAssembly);
        }
        public virtual void Shutdown()
        {
            Singleton<CommandManager>.Instance.UnRegisterAll(this.Context.PluginAssembly);
        }
        public abstract void Dispose();
        public virtual void LoadConfig()
        {
            if (this.UseConfig)
            {
                string configPath = this.GetConfigPath();
                this.Config = new Config(configPath);
                this.Config.AddAssembly(base.GetType().Assembly);
                if (File.Exists(configPath))
                {
                    this.Config.Load();
                }
                else
                {
                    this.Config.Create(false);
                }
            }
        }
        public virtual string GetConfigPath()
        {
            return Path.Combine(this.GetPluginDirectory(), (!string.IsNullOrEmpty(this.ConfigFileName)) ? this.ConfigFileName : (this.Name.Replace(" ","_") + ".xml"));
        }
        public string GetPluginDirectory()
        {
            return Path.GetDirectoryName(this.Context.AssemblyPath + "/../config/");
        }
    }
}
