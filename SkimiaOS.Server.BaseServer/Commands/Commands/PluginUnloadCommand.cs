using SkimiaOS.Core.Reflection;
using SkimiaOS.Server.BaseServer.Commands.Enums;
using SkimiaOS.Server.BaseServer.Plugins;
using System;
namespace SkimiaOS.Server.BaseServer.Commands.Commands
{
	public class PluginUnloadCommand : SubCommand
	{
		public PluginUnloadCommand()
		{
			base.ParentCommand = typeof(PluginsCommand);
			base.Aliases = new string[]
			{
				"unload"
			};
			base.RequiredRole = RoleEnum.Administrator;
			base.Description = "Unload a plugin";
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
				trigger.Reply("Plugin {0} unloaded", new object[]
				{
					plugin.Plugin.Name
				});
			}
		}
	}
}
