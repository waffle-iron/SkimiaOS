using System;
namespace SkimiaOS.Server.BaseServer.Commands
{
	public abstract class SubCommand : CommandBase
	{
		public Type ParentCommand
		{
			get;
			protected set;
		}
		public override string ToString()
		{
			return base.GetType().Name;
		}
	}
}
