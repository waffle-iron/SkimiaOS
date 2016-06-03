using SkimiaOS.Core.Reflection;
using SkimiaOS.Server.BaseServer.Commands.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
namespace SkimiaOS.Server.BaseServer.Commands.Commands
{
	public class ListCommand : CommandBase
	{
		public ListCommand()
		{
			base.Aliases = new string[]
			{
				"commandslist"
			};
			base.RequiredRole = RoleEnum.User;
			base.Description = "List all available commands";
			base.Parameters = new List<IParameterDefinition>
			{
				new ParameterDefinition<string>("command", "cmd", "List specifics sub commands", string.Empty, false, null),
				new ParameterDefinition<RoleEnum>("role", "role", "List commands available for a given role", RoleEnum.None, true, null)
			};
		}
		public override void Execute(TriggerBase trigger)
		{
			RoleEnum role = trigger.Get<RoleEnum>("role");
			string text = trigger.Get<string>("command");
			role = ((role == RoleEnum.None || role > trigger.UserRole) ? trigger.UserRole : role);
			IEnumerable<CommandBase> source = 
				from entry in Singleton<CommandManager>.Instance.AvailableCommands
				where !(entry is SubCommand)
				select entry;
			if (text != string.Empty)
			{
				CommandBase command = Singleton<CommandManager>.Instance.GetCommand(text);
				if (command == null)
				{
					trigger.ReplyError("Cannot found '{0}'", new object[]
					{
						command
					});
					return;
				}
				if (command is SubCommandContainer)
				{
					source = (command as SubCommandContainer);
				}
				else
				{
					source = new CommandBase[]
					{
						command
					};
				}
			}
			IEnumerable<CommandBase> source2 = 
				from entry in source
				where entry.RequiredRole <= role
				select entry;
			trigger.Reply(string.Join(", ", 
				from entry in source2
				select (entry is SubCommandContainer) ? string.Format(trigger.CanFormat ? "<b>{0}</b>({1})" : "{0}({1})", entry.Aliases.First<string>(), (entry as SubCommandContainer).Count) : entry.Aliases.First<string>()));
		}
	}
}
