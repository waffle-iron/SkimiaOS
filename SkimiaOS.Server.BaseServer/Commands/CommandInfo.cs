using SkimiaOS.Server.BaseServer.Commands.Enums;
using System;
using System.Xml.Serialization;
namespace SkimiaOS.Server.BaseServer.Commands
{
	[Serializable]
	public class CommandInfo
	{
		[XmlAttribute("name")]
		public string Name
		{
			get;
			set;
		}
		[XmlElement]
		public RoleEnum RequiredRole
		{
			get;
			set;
		}
		[XmlElement]
		public string Description
		{
			get;
			set;
		}
		[XmlElement]
		public string Usage
		{
			get;
			set;
		}
		public CommandInfo()
		{
		}
		public CommandInfo(CommandBase command)
		{
			this.Name = command.GetType().Name;
			this.RequiredRole = command.RequiredRole;
			this.Description = command.Description;
		}
		public void ModifyCommandInfo(CommandBase command)
		{
			command.RequiredRole = this.RequiredRole;
			command.Description = this.Description;
			command.Usage = this.Usage;
		}
	}
}
