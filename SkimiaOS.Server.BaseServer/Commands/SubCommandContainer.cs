using SkimiaOS.Server.BaseServer.Commands.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace SkimiaOS.Server.BaseServer.Commands
{
	public abstract class SubCommandContainer : CommandBase, IEnumerable<SubCommand>, IEnumerable
	{
		private readonly List<SubCommand> m_subCommands = new List<SubCommand>();
		public SubCommand this[string name]
		{
			get
			{
				SubCommand subCommand;
				return (!this.TryGetSubCommand(name, out subCommand)) ? null : subCommand;
			}
		}
		public int Count
		{
			get
			{
				return this.m_subCommands.Count;
			}
		}
		public IEnumerator<SubCommand> GetEnumerator()
		{
			return this.m_subCommands.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		public override void Execute(TriggerBase trigger)
		{
			string text = trigger.Args.NextWord();
			SubCommand subCommand;
			if (!this.TryGetSubCommand(CommandBase.IgnoreCommandCase ? text.ToLower() : text, out subCommand) || subCommand.RequiredRole > trigger.UserRole)
			{
				HelpCommand.DisplayFullCommandDescription(trigger, this);
			}
			else
			{
				if (trigger.BindToCommand(subCommand))
				{
					subCommand.Execute(trigger);
				}
			}
		}
		public void AddSubCommand(SubCommand subCommand)
		{
			this.m_subCommands.Add(subCommand);
		}
		public void RemoveSubCommand(SubCommand subCommand)
		{
			this.m_subCommands.Remove(subCommand);
		}
		public void SortSubCommands()
		{
			this.m_subCommands.Sort((SubCommand subcmd1, SubCommand subcmd2) => subcmd1.Aliases.First<string>().CompareTo(subcmd2.Aliases.First<string>()));
		}
		public bool TryGetSubCommand(string subcmd, out SubCommand result)
		{
			IEnumerable<SubCommand> source = 
				from sub in this.m_subCommands
				from subalias in sub.Aliases
				where subalias.Equals(subcmd, CommandBase.IgnoreCommandCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture)
				select sub;
			result = source.SingleOrDefault<SubCommand>();
			return result != null;
		}
	}
}
