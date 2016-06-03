using SkimiaOS.Server.BaseServer.Commands.Enums;
using System;
namespace SkimiaOS.Server.BaseServer.Commands.Commands
{
	public class PluginsCommand : SubCommandContainer
	{
		public PluginsCommand()
		{
			base.Aliases = new string[]
			{
				"plugins"
			};
			base.RequiredRole = RoleEnum.Administrator;
			base.Description = "Provide commands to manage plugins";
		}
	}
}
