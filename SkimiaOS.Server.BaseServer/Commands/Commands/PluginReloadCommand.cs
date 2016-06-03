using SkimiaOS.Core.Reflection;
using SkimiaOS.Server.BaseServer.Commands.Enums;
using SkimiaOS.Server.BaseServer.Plugins;
using System;
namespace SkimiaOS.Server.BaseServer.Commands.Commands
{
	public class PluginReloadCommand : SubCommand
	{
		public PluginReloadCommand()
		{
			base.ParentCommand = typeof(PluginsCommand);
			base.Aliases = new string[]
			{
				"reload"
			};
			base.RequiredRole = RoleEnum.Administrator;
			base.Description = "Reload and reset a plugin";
			base.AddParameter<string>("name", "n", "Plugin name", null, false, null);
			base.AddParameter<bool>("ignoreCase", "case", "Ignore case for name comparaison", true, false, null);
		}
		public override void Execute(TriggerBase trigger)
		{
			PluginContext plugin = Singleton<PluginManager>.Instance.GetPlugin(trigger.Get<string>("name"), trigger.Get<bool>("case"));
			if (plugin == null)
			{
				trigger.ReplyError("Plugin not found");
			}
			else
			{
				Singleton<PluginManager>.Instance.UnLoadPlugin(plugin);
				Singleton<PluginManager>.Instance.LoadPlugin(plugin.AssemblyPath);
				trigger.Reply("Plugin {0} reloaded", new object[]
				{
					plugin.Plugin.Name
				});
			}
		}
	}
}
