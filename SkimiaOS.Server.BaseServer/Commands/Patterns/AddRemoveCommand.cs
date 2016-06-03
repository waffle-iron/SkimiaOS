using System;
namespace SkimiaOS.Server.BaseServer.Commands.Patterns
{
	public abstract class AddRemoveCommand : CommandBase
	{
		protected AddRemoveCommand()
		{
			base.AddParameter<string>("action", "action", "Add/Remove", null, false, null);
		}
		public override void Execute(TriggerBase trigger)
		{
			string text = trigger.Get<string>("action");
			if (text.Equals("add", StringComparison.InvariantCultureIgnoreCase))
			{
				this.ExecuteAdd(trigger);
			}
			else
			{
				if (text.Equals("remove", StringComparison.InvariantCultureIgnoreCase) || text.Equals("rm", StringComparison.InvariantCultureIgnoreCase))
				{
					this.ExecuteRemove(trigger);
				}
				else
				{
					trigger.ReplyError("Invalid action {0}, define add or remove", new object[]
					{
						text
					});
				}
			}
		}
		public abstract void ExecuteAdd(TriggerBase trigger);
		public abstract void ExecuteRemove(TriggerBase trigger);
	}
}
