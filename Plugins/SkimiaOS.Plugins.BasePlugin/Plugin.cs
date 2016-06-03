using SkimiaOS.Server.BaseServer.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Plugins.BasePlugin
{
    public class Plugin : PluginBase
    {
        public Plugin(PluginContext context)
            : base(context)
        {
            if (CurrentPlugin != null)
                throw new Exception("Can be instancied only once");

            CurrentPlugin = this;
        }

        public static Plugin CurrentPlugin
        {
            get;
            private set;
        }

        public override string Name
        {
            get
            {
                return "Base Plugin";
            }
        }

        public override string Description
        {
            get
            {
                return "create a test page for owin";
            }
        }

        public override string Author
        {
            get
            {
                return "kessler";
            }
        }

        public override Version Version
        {
            get
            {
                return new Version(1, 0);
            }
        }

        public override bool UseConfig
        {
            get
            {
                return true;
            }
        }

        public override void Dispose()
        {
        }
    }
}
