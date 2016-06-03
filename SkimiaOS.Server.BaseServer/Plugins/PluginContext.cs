using System;
using System.Reflection;

namespace SkimiaOS.Server.BaseServer.Plugins
{
    public class PluginContext
    {
        public string AssemblyPath
        {
            get;
            private set;
        }
        public Assembly PluginAssembly
        {
            get;
            private set;
        }
        public IPlugin Plugin
        {
            get;
            private set;
        }
        public PluginContext(string assemblyPath, Assembly pluginAssembly)
        {
            this.AssemblyPath = assemblyPath;
            this.PluginAssembly = pluginAssembly;
        }
        internal void Initialize(Type pluginType)
        {
            this.Plugin = (IPlugin)Activator.CreateInstance(pluginType, new object[]
            {
                this
            });
            if (this.Plugin != null)
            {
                this.Plugin.LoadConfig();
                this.Plugin.Initialize();
            }
        }
        public override string ToString()
        {
            string result;
            if (this.Plugin == null)
            {
                result = this.PluginAssembly.FullName;
            }
            else
            {
                result = this.Plugin.Name + " : " + this.Plugin.Description;
            }
            return result;
        }
    }
}
