using SkimiaOS.Core.Reflection;
using SkimiaOS.Server.BaseServer.Commands.Enums;
using SkimiaOS.Server.BaseServer.Plugins;
using System;
namespace SkimiaOS.Server.BaseServer.Commands.Commands
{
	public class PluginListCommand : SubCommand
	{
		public PluginListCommand()
		{
			base.ParentCommand = typeof(PluginsCommand);
			base.Aliases = new string[]
			{
				"list"
			};
			base.RequiredRole = RoleEnum.Administrator;
			base.Description = "List all loaded plugins";
		}
		public override void Execute(TriggerBase trigger)
		{
			PluginContext[] plugins = Singleton<PluginManager>.Instance.GetPlugins();
			if (plugins.Length == 0)
			{
				trigger.Reply("No plugin loaded");
			}
			PluginContext[] array = plugins;
			for (int i = 0; i < array.Length; i++)
			{
				PluginContext pluginContext = array[i];
				trigger.Reply("- {0} : {1}", new object[]
				{
					pluginContext.Plugin.Name,
					pluginContext.Plugin.Description
				});
			}
		}
	}
}
