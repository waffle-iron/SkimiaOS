using SkimiaOS.Core.Messages;
using SkimiaOS.Server.BaseServer.Plugins;

namespace SkimiaOS.Server.BaseServer.Messages
{
    public class PluginRemovedMessage : Message
    {
        public PluginRemovedMessage(PluginContext plugin)
        {
            PluginContext = plugin;
        }

        public PluginContext PluginContext
        {
            get;
            set;
        }
    }
}
