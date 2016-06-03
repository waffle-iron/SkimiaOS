using SkimiaOS.Core.Reflection;
using SkimiaOS.Server.BaseServer.Commands.Enums;
using SkimiaOS.Server.BaseServer.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace SkimiaOS.Server.BaseServer.Commands.Commands
{
	public class PluginLoadCommand : SubCommand
	{
		public PluginLoadCommand()
		{
			base.ParentCommand = typeof(PluginsCommand);
			base.Aliases = new string[]
			{
				"load"
			};
			base.RequiredRole = RoleEnum.Administrator;
			base.Description = "Load a plugin";
			base.AddParameter<string>("name", "n", "Plugin name or path (in plugins directory)", null, false, null);
		}
		public override void Execute(TriggerBase trigger)
		{
			string text = trigger.Get<string>("name");
			if (Directory.Exists(text))
			{
				using (IEnumerator<string> enumerator = Directory.EnumerateFiles(text, "*.dll", text.EndsWith("*") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string file = enumerator.Current;
						IEnumerable<PluginContext> arg_6D_0 = Singleton<PluginManager>.Instance.GetPlugins();
						Func<PluginContext, bool> predicate = (PluginContext entry) => Path.GetFullPath(entry.AssemblyPath) == Path.GetFullPath(file);
						if (!arg_6D_0.Any(predicate))
						{
							PluginContext pluginContext = Singleton<PluginManager>.Instance.LoadPlugin(file);
							trigger.Reply("Plugin {0} loaded", new object[]
							{
								pluginContext.Plugin.Name
							});
						}
					}
					return;
				}
			}
			if (File.Exists(text))
			{
				PluginContext pluginContext = Singleton<PluginManager>.Instance.LoadPlugin(text);
				trigger.Reply("Plugin {0} loaded", new object[]
				{
					pluginContext.Plugin.Name
				});
			}
			else
			{
				foreach (string current in PluginManager.PluginsPath)
				{
					if (Directory.Exists(current))
					{
						using (IEnumerator<string> enumerator = Directory.EnumerateFiles(current, "*.dll", SearchOption.AllDirectories).GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								string plugin = enumerator.Current;
								IEnumerable<PluginContext> arg_17D_0 = Singleton<PluginManager>.Instance.GetPlugins();
								Func<PluginContext, bool> predicate2 = (PluginContext entry) => Path.GetFullPath(entry.AssemblyPath) == Path.GetFullPath(plugin);
								if (!arg_17D_0.Any(predicate2) && (Path.GetFileNameWithoutExtension(plugin).Equals(Path.GetFileNameWithoutExtension(text), StringComparison.InvariantCultureIgnoreCase) || Path.GetFileName(plugin).Equals(Path.GetFileName(plugin), StringComparison.InvariantCultureIgnoreCase)))
								{
									PluginContext pluginContext2 = Singleton<PluginManager>.Instance.LoadPlugin(plugin);
									trigger.Reply("Plugin {0} loaded", new object[]
									{
										pluginContext2.Plugin.Name
									});
								}
							}
						}
					}
				}
			}
		}
	}
}
