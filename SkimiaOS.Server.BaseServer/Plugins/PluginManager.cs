using NLog;
using SkimiaOS.Core.Config;
using SkimiaOS.Core.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SkimiaOS.Server.BaseServer.Plugins
{
    public sealed class PluginManager : Singleton<PluginManager>
    {
        public delegate void PluginContextHandler(PluginContext pluginContext);
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        [Configurable(true)]
        public static List<string> PluginsPath = new List<string>
        {
            "./core/",
            "./plugins/"
        };
        internal readonly IList<PluginContext> PluginContexts = new List<PluginContext>();
        public event PluginManager.PluginContextHandler PluginAdded;
        public event PluginManager.PluginContextHandler PluginRemoved;
        private void InvokePluginAdded(PluginContext pluginContext)
        {
            PluginManager.PluginContextHandler pluginAdded = this.PluginAdded;
            if (pluginAdded != null)
            {
                pluginAdded(pluginContext);
            }
        }
        private void InvokePluginRemoved(PluginContext pluginContext)
        {
            PluginManager.PluginContextHandler pluginRemoved = this.PluginRemoved;
            if (pluginRemoved != null)
            {
                pluginRemoved(pluginContext);
            }
        }
        private PluginManager()
        {
        }
        public void LoadAllPlugins()
        {
            foreach (string current in PluginManager.PluginsPath)
            {
                if (!Directory.Exists(current) && !File.Exists(current))
                {
                    PluginManager.logger.Error("Cannot load unexistant plugin path {0}", current);
                    continue;
                }
                if (File.GetAttributes(current).HasFlag(FileAttributes.Directory))
                {
                    using (IEnumerator<string> enumerator2 = Directory.EnumerateFiles(current, "*.dll").GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            string current2 = enumerator2.Current;
                            this.LoadPlugin(current2);
                        }
                        continue;
                    }
                }
                this.LoadPlugin(current);
            }
        }
        public PluginContext LoadPlugin(string libPath)
        {
            if (!File.Exists(libPath))
            {
                throw new FileNotFoundException("File doesn't exist", libPath);
            }
            if (this.PluginContexts.Any((PluginContext entry) => Path.GetFullPath(entry.AssemblyPath) == Path.GetFullPath(libPath)))
            {
                throw new Exception("Plugin already loaded");
            }
            byte[] rawAssembly = File.ReadAllBytes(libPath);
            Assembly assembly = Assembly.Load(rawAssembly);
            PluginContext pluginContext = new PluginContext(libPath, assembly);
            bool flag = false;
            foreach (Type current in
                from pluginType in assembly.GetTypes()
                where pluginType.IsPublic && !pluginType.IsAbstract
                where pluginType.HasInterface(typeof(IPlugin))
                select pluginType)
            {
                if (flag)
                {
                    throw new PluginLoadException("Found 2 classes that implements IPlugin. A plugin can contains only one");
                }
                pluginContext.Initialize(current);
                flag = true;
                this.RegisterPlugin(pluginContext);
            }
            return pluginContext;
        }
        public void UnLoadPlugin(string name, bool ignoreCase = false)
        {
            IEnumerable<PluginContext> enumerable =
                from entry in this.PluginContexts
                where entry.Plugin.Name.Equals(name, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture)
                select entry;
            foreach (PluginContext current in enumerable)
            {
                this.UnLoadPlugin(current);
            }
        }
        public void UnLoadPlugin(PluginContext context)
        {
            context.Plugin.Shutdown();
            context.Plugin.Dispose();
            this.UnRegisterPlugin(context);
        }
        public PluginContext GetPlugin(string name, bool ignoreCase = false)
        {
            IEnumerable<PluginContext> source =
                from entry in this.PluginContexts
                where entry.Plugin.Name.Equals(name, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture)
                select entry;
            return source.FirstOrDefault<PluginContext>();
        }
        public PluginContext[] GetPlugins()
        {
            return this.PluginContexts.ToArray<PluginContext>();
        }
        internal void RegisterPlugin(PluginContext pluginContext)
        {
            this.PluginContexts.Add(pluginContext);
            this.InvokePluginAdded(pluginContext);
        }
        internal void UnRegisterPlugin(PluginContext pluginContext)
        {
            this.PluginContexts.Remove(pluginContext);
            this.InvokePluginRemoved(pluginContext);
        }
    }
}
