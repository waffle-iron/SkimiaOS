using System;
namespace SkimiaOS.Server.BaseServer.Commands
{
	public delegate T ConverterHandler<out T>(string str, TriggerBase trigger);
}
