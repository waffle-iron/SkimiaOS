using NLog;
using SkimiaOS.Core.Reflection;
using SkimiaOS.Core.Xml;
using SkimiaOS.Server.BaseServer.Commands.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
namespace SkimiaOS.Server.BaseServer.Commands
{
	public class CommandManager : Singleton<CommandManager>
	{
		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		private IDictionary<string, CommandBase> m_commandsByAlias;
		private readonly List<CommandBase> m_registeredCommands;
		private readonly List<Type> m_registeredTypes;
		private readonly List<CommandInfo> m_commandsInfos;
		public IDictionary<string, CommandBase> CommandsByAlias
		{
			get
			{
				return this.m_commandsByAlias;
			}
		}
		public IReadOnlyCollection<CommandBase> AvailableCommands
		{
			get
			{
				return this.m_registeredCommands.AsReadOnly();
			}
		}
		public CommandBase this[string alias]
		{
			get
			{
				return this.GetCommand(alias);
			}
		}
		private CommandManager()
		{
			this.m_commandsByAlias = new Dictionary<string, CommandBase>();
			this.m_registeredCommands = new List<CommandBase>();
			this.m_registeredTypes = new List<Type>();
			this.m_commandsInfos = new List<CommandInfo>();
		}
		public CommandBase GetCommand(string alias)
		{
			CommandBase result;
			this.m_commandsByAlias.TryGetValue(alias, out result);
			return result;
		}
		public void RegisterAll(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			IEnumerable<Type> enumerable = 
				from entry in assembly.GetTypes()
				where !entry.IsAbstract
				select entry;
			foreach (Type current in enumerable)
			{
				if (!this.IsCommandRegister(current))
				{
					this.RegisterCommand(current);
				}
			}
			this.SortCommands();
		}
		public void LoadOrCreateCommandsInfo(string xmlFile)
		{
			if (!File.Exists(xmlFile))
			{
				XmlUtils.Serialize<List<CommandInfo>>(xmlFile, this.m_commandsInfos);
			}
			else
			{
				CommandInfo[] infos = XmlUtils.Deserialize<CommandInfo[]>(xmlFile);
				this.LoadCommandsInfo(infos);
			}
		}
		public void LoadCommandsInfo(CommandInfo[] infos)
		{
			for (int i = 0; i < infos.Length; i++)
			{
				CommandInfo info = infos[i];
				List<CommandInfo> arg_31_0 = this.m_commandsInfos;
				Predicate<CommandInfo> match = (CommandInfo x) => x.Name == info.Name;
				if (arg_31_0.RemoveAll(match) > 0)
				{
					this.m_commandsInfos.Add(info);
				}
				CommandBase commandBase = this.AvailableCommands.FirstOrDefault((CommandBase x) => x.GetType().Name == info.Name);
				if (commandBase != null)
				{
					info.ModifyCommandInfo(commandBase);
				}
			}
		}
		public void RegisterCommand(Type commandType)
		{
			if (!this.IsCommandRegister(commandType))
			{
				if (commandType.IsSubclassOf(typeof(SubCommand)))
				{
					this.RegisterSubCommand(commandType);
				}
				else
				{
					if (commandType.IsSubclassOf(typeof(SubCommandContainer)))
					{
						this.RegisterSubCommandContainer(commandType);
					}
					else
					{
						if (commandType.IsSubclassOf(typeof(CommandBase)))
						{
							this.RegisterCommandBase(commandType);
						}
					}
				}
			}
		}
		private void RegisterCommandBase(Type commandType)
		{
			CommandBase commandBase = Activator.CreateInstance(commandType) as CommandBase;
			if (commandBase == null)
			{
				throw new Exception(string.Format("Cannot create a new instance of {0}", commandType));
			}
			if (commandBase.Aliases == null)
			{
				this.logger.Error("Cannot register Command {0}, Aliases is null", commandType.Name);
			}
			else
			{
				if (commandBase.RequiredRole == RoleEnum.None)
				{
					this.logger.Error("Cannot register Command : {0}. RequiredRole is incorrect", commandType.Name);
				}
				else
				{
					this.m_registeredCommands.Add(commandBase);
					this.m_commandsInfos.Add(new CommandInfo(commandBase));
					this.m_registeredTypes.Add(commandType);
					string[] aliases = commandBase.Aliases;
					for (int i = 0; i < aliases.Length; i++)
					{
						string text = aliases[i];
						CommandBase argument;
						if (!this.m_commandsByAlias.TryGetValue(text, out argument))
						{
							this.m_commandsByAlias[CommandBase.IgnoreCommandCase ? text.ToLower() : text] = commandBase;
						}
						else
						{
							this.logger.Error<string, CommandBase, CommandBase>("Found two Commands with Alias \"{0}\": {1} and {2}", text, argument, commandBase);
						}
					}
				}
			}
		}
		private void RegisterSubCommandContainer(Type commandType)
		{
			SubCommandContainer subCommandContainer = Activator.CreateInstance(commandType) as SubCommandContainer;
			if (subCommandContainer == null)
			{
				throw new Exception(string.Format("Cannot create a new instance of {0}", commandType));
			}
			if (subCommandContainer.Aliases == null)
			{
				this.logger.Error("Cannot register Command {0}, Aliases is null", commandType.Name);
			}
			else
			{
				if (subCommandContainer.RequiredRole == RoleEnum.None)
				{
					this.logger.Error("Cannot register Command : {0}. RequiredRole is incorrect", commandType.Name);
				}
				else
				{
					this.m_registeredCommands.Add(subCommandContainer);
					this.m_commandsInfos.Add(new CommandInfo(subCommandContainer));
					this.m_registeredTypes.Add(commandType);
					string[] aliases = subCommandContainer.Aliases;
					for (int i = 0; i < aliases.Length; i++)
					{
						string text = aliases[i];
						CommandBase argument;
						if (!this.m_commandsByAlias.TryGetValue(text, out argument))
						{
							this.m_commandsByAlias[CommandBase.IgnoreCommandCase ? text.ToLower() : text] = subCommandContainer;
						}
						else
						{
							this.logger.Error<string, CommandBase, SubCommandContainer>("Found two Commands with Alias \"{0}\": {1} and {2}", text, argument, subCommandContainer);
						}
					}
				}
			}
		}
		private void RegisterSubCommand(Type commandType)
		{
			SubCommand subcommand = Activator.CreateInstance(commandType) as SubCommand;
			if (subcommand == null)
			{
				throw new Exception(string.Format("Cannot create a new instance of {0}", commandType));
			}
			if (subcommand.Aliases == null)
			{
				this.logger.Error("Cannot register subcommand {0}, Aliases is null", commandType.Name);
			}
			else
			{
				if (subcommand.RequiredRole == RoleEnum.None)
				{
					this.logger.Error("Cannot register subcommand : {0}. RequiredRole is incorrect", commandType.Name);
				}
				else
				{
					if (subcommand.ParentCommand == null)
					{
						this.logger.Error<Type>("The subcommand {0} has no parent command and cannot be registered", commandType);
					}
					else
					{
						if (!this.IsCommandRegister(subcommand.ParentCommand))
						{
							this.RegisterCommand(subcommand.ParentCommand);
						}
						SubCommandContainer subCommandContainer = (
							from entry in this.AvailableCommands
							where entry.GetType() == subcommand.ParentCommand
							select entry).SingleOrDefault<CommandBase>() as SubCommandContainer;
						if (subCommandContainer == null)
						{
							throw new Exception(string.Format("Cannot found declaration of command '{0}'", subcommand.ParentCommand));
						}
						subCommandContainer.AddSubCommand(subcommand);
						this.m_registeredCommands.Add(subcommand);
						this.m_commandsInfos.Add(new CommandInfo(subcommand));
						this.m_registeredTypes.Add(commandType);
					}
				}
			}
		}
		public void UnRegisterAll(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			IEnumerable<Type> enumerable = 
				from entry in assembly.GetTypes()
				where !entry.IsAbstract
				select entry;
			foreach (Type current in enumerable)
			{
				if (this.IsCommandRegister(current))
				{
					this.UnRegisterCommand(current);
				}
			}
		}
		public void UnRegisterCommand(Type commandType)
		{
			CommandBase commandBase = Activator.CreateInstance(commandType) as CommandBase;
			if (commandBase != null)
			{
				string[] aliases = commandBase.Aliases;
				for (int i = 0; i < aliases.Length; i++)
				{
					string alias = aliases[i];
					this.m_commandsByAlias.Remove(alias);
					List<CommandInfo> arg_77_0 = this.m_commandsInfos;
					Predicate<CommandInfo> match = (CommandInfo x) => x.Name == alias;
					arg_77_0.RemoveAll(match);
				}
				this.m_registeredTypes.Remove(commandType);
				this.m_registeredCommands.RemoveAll((CommandBase entry) => entry.GetType() == commandType);
			}
		}
		private void SortCommands()
		{
			this.m_commandsByAlias = (
				from entry in this.m_commandsByAlias
				orderby entry.Key
				select entry).ToDictionary((KeyValuePair<string, CommandBase> entry) => entry.Key, (KeyValuePair<string, CommandBase> entry) => entry.Value);
			foreach (SubCommandContainer current in this.AvailableCommands.OfType<SubCommandContainer>())
			{
				current.SortSubCommands();
			}
		}
		public bool IsCommandRegister(Type commandType)
		{
			return this.m_registeredTypes.Contains(commandType);
		}
		public void HandleCommand(TriggerBase trigger)
		{
			string text = trigger.Args.NextWord();
			if (CommandBase.IgnoreCommandCase)
			{
				text = text.ToLower();
			}
			CommandBase commandBase = this[text];
			if (commandBase != null && trigger.CanAccessCommand(commandBase))
			{
				try
				{
					if (trigger.BindToCommand(commandBase))
					{
						commandBase.Execute(trigger);
					}
					return;
				}
				catch (Exception ex)
				{
					trigger.ReplyError("Raised exception (error-index:{0}) : {1}", new object[]
					{
						trigger.RegisterException(ex),
						ex.Message
					});
					if (ex.InnerException != null)
					{
						trigger.ReplyError(" => " + ex.InnerException.Message);
					}
					return;
				}
			}
			trigger.ReplyError("Incorrect Command \"{0}\". Type commandslist or help for command list.", new object[]
			{
				text
			});
		}
	}
}
