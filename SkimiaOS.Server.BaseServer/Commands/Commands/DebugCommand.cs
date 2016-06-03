using SkimiaOS.Server.BaseServer.Commands.Enums;
using System;
namespace SkimiaOS.Server.BaseServer.Commands.Commands
{
	public class DebugCommand : SubCommandContainer
	{
		public DebugCommand()
		{
			base.Aliases = new string[]
			{
				"debug"
			};
			base.RequiredRole = RoleEnum.Administrator;
			base.Description = "Provides command to debug things";
		}
	}
}
