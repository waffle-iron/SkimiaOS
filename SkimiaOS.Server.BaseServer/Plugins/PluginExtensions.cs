

namespace SkimiaOS.Server.BaseServer.Plugins
{
    public static class PluginExtensions
    {
        public static string GetDefaultDescription(this IPlugin plugin)
        {
            return string.Format("'{0}' v{1} by {2}", plugin.Name, plugin.GetType().Assembly.GetName().Version, plugin.Author);
        }
    }
}
