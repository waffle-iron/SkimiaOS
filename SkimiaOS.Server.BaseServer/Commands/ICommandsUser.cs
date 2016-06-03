using System;
using System.Collections.Generic;
namespace SkimiaOS.Server.BaseServer.Commands
{
	public interface ICommandsUser
	{
		List<KeyValuePair<string, Exception>> CommandsErrors
		{
			get;
		}
	}
}
