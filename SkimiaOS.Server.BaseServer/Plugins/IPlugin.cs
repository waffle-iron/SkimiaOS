using System;


namespace SkimiaOS.Server.BaseServer.Plugins
{
    public interface IPlugin
    {
        PluginContext Context
        {
            get;
        }
        string Name
        {
            get;
        }
        string Description
        {
            get;
        }
        string Author
        {
            get;
        }
        Version Version
        {
            get;
        }
        void LoadConfig();
        void Initialize();
        void Shutdown();
        void Dispose();
    }
}
