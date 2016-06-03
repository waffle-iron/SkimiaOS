using System;

namespace SkimiaOS.Server.BaseServer.Plugins
{
    public class PluginLoadException : Exception
    {
        public PluginLoadException(string exception) : base(exception)
        {
        }
    }
}