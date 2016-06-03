using SkimiaOS.Core.Reflection;
using SkimiaOS.Server.BaseServer.Commands.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
namespace SkimiaOS.Server.BaseServer.Commands.Commands
{
	public class HelpCommand : CommandBase
	{
		public HelpCommand()
		{
			base.Aliases = new string[]
			{
				"help",
				"?"
			};
			base.RequiredRole = RoleEnum.User;
			base.Description = "List all available commands";
			base.Parameters = new List<IParameterDefinition>
			{
				new ParameterDefinition<string>("command", "cmd", "Display the complete help of a command", string.Empty, false, null),
				new ParameterDefinition<string>("subcommand", "subcmd", "Display the complete help of a subcommand", string.Empty, false, null)
			};
		}
		public override void Execute(TriggerBase trigger)
		{
			string text = trigger.Get<string>("command");
			string text2 = trigger.Get<string>("subcmd");
			CommandBase commandBase;
			if (text == string.Empty)
			{
				using (IEnumerator<CommandBase> enumerator = Singleton<CommandManager>.Instance.AvailableCommands.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						commandBase = enumerator.Current;
						if (!(commandBase is SubCommand) && commandBase.RequiredRole <= trigger.UserRole)
						{
							HelpCommand.DisplayCommandDescription(trigger, commandBase);
						}
					}
					return;
				}
			}
			commandBase = Singleton<CommandManager>.Instance.GetCommand(text);
			if (commandBase == null || commandBase.RequiredRole > trigger.UserRole)
			{
				trigger.Reply("Command '{0}' doesn't exist", new object[]
				{
					text
				});
			}
			else
			{
				if (text2 == string.Empty)
				{
					HelpCommand.DisplayFullCommandDescription(trigger, commandBase);
				}
				else
				{
					if (!(commandBase is SubCommandContainer))
					{
						trigger.Reply("Command '{0}' has no sub commands", new object[]
						{
							text
						});
					}
					else
					{
						SubCommand subCommand = (commandBase as SubCommandContainer)[text2];
						if (subCommand == null || subCommand.RequiredRole > trigger.UserRole)
						{
							trigger.Reply("Command '{0} {1}' doesn't exist", new object[]
							{
								text,
								text2
							});
						}
						else
						{
							HelpCommand.DisplayFullSubCommandDescription(trigger, commandBase, subCommand);
						}
					}
				}
			}
		}
		public static void DisplayCommandDescription(TriggerBase trigger, CommandBase command)
		{
			trigger.Reply(trigger.Bold("{0}") + "{1} - {2}", new object[]
			{
				string.Join("/", command.Aliases),
				(command is SubCommandContainer) ? string.Format(" ({0} subcmds)", (command as SubCommandContainer).Count((SubCommand entry) => entry.RequiredRole <= trigger.UserRole)) : "",
				command.Description
			});
		}
		public static void DisplaySubCommandDescription(TriggerBase trigger, CommandBase command, SubCommand subcommand)
		{
			trigger.Reply(trigger.Bold("{0}") + " {1} - {2}", new object[]
			{
				command.Aliases.First<string>(),
				string.Join("/", subcommand.Aliases),
				subcommand.Description
			});
		}
		public static void DisplayFullCommandDescription(TriggerBase trigger, CommandBase command)
		{
			trigger.Reply(trigger.Bold("{0}") + "{1} - {2}", new object[]
			{
				string.Join("/", command.Aliases),
				(!(command is SubCommandContainer) || (command as SubCommandContainer).Count <= 0) ? "" : string.Format(" ({0} subcmds)", (command as SubCommandContainer).Count((SubCommand entry) => entry.RequiredRole <= trigger.UserRole)),
				command.Description
			});
			if (!(command is SubCommandContainer))
			{
				trigger.Reply("  -> " + command.Aliases.First<string>() + " " + command.GetSafeUsage());
			}
			if (command.Parameters != null)
			{
				foreach (IParameterDefinition current in command.Parameters)
				{
					HelpCommand.DisplayCommandParameter(trigger, current);
				}
			}
			if (command is SubCommandContainer)
			{
				foreach (SubCommand current2 in command as SubCommandContainer)
				{
					HelpCommand.DisplayFullSubCommandDescription(trigger, command, current2);
				}
			}
		}
		public static void DisplayFullSubCommandDescription(TriggerBase trigger, CommandBase command, SubCommand subcommand)
		{
			trigger.Reply(trigger.Bold("{0} {1}") + " - {2}", new object[]
			{
				command.Aliases.First<string>(),
				string.Join("/", subcommand.Aliases),
				subcommand.Description
			});
			trigger.Reply(string.Concat(new string[]
			{
				"  -> ",
				command.Aliases.First<string>(),
				" ",
				subcommand.Aliases.First<string>(),
				" ",
				subcommand.GetSafeUsage()
			}));
			foreach (IParameterDefinition current in subcommand.Parameters)
			{
				HelpCommand.DisplayCommandParameter(trigger, current);
			}
		}
		public static void DisplayCommandParameter(TriggerBase trigger, IParameterDefinition parameter)
		{
			trigger.Reply("\t(" + trigger.Bold("{0}") + " : {1})", new object[]
			{
				parameter.GetUsage(),
				parameter.Description ?? ""
			});
		}
	}
}
