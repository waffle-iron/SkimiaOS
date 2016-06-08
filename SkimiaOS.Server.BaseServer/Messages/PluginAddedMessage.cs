using SkimiaOS.Core.Messages;
using SkimiaOS.Server.BaseServer.Plugins;

namespace SkimiaOS.Server.BaseServer.Messages
{
    public class PluginAddedMessage : Message
    {
        public PluginAddedMessage(PluginContext plugin)
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
