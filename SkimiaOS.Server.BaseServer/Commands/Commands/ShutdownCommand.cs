using SkimiaOS.Server.BaseServer.Commands.Enums;
using System;
using System.Threading;
namespace SkimiaOS.Server.BaseServer.Commands.Commands
{
	public class ShutdownCommand : CommandBase
	{
		private int m_shutdownCountdown;
		public ShutdownCommand()
		{
			base.Aliases = new string[]
			{
				"shutdown",
				"stop"
			};
			base.RequiredRole = RoleEnum.Administrator;
			base.Description = "Stop the server";
			base.Usage = "";
			base.AddParameter<int>("time", "t", "Stop after [time] seconds", 0, false, null);
			base.AddParameter<string>("reason", "r", "Display a reason for the shutdown", null, true, null);
			base.AddParameter<bool>("cancel", "c", "Cancel a shutting down procedure", false, true, null);
		}
		public override void Execute(TriggerBase trigger)
		{
			if (trigger.Get<bool>("cancel"))
			{
				ServerBase.InstanceAsBase.CancelScheduledShutdown();
				trigger.Reply("Shutting down procedure is canceled.");
			}
			else
			{
				this.m_shutdownCountdown = trigger.Get<int>("time");
				if (this.m_shutdownCountdown > 0)
				{
					ServerBase.InstanceAsBase.ScheduleShutdown(TimeSpan.FromSeconds((double)this.m_shutdownCountdown), trigger.Get<string>("reason"));
					trigger.Reply("Server shutting down in {0} seconds", new object[]
					{
						this.m_shutdownCountdown
					});
				}
				else
				{
					ServerBase.InstanceAsBase.Shutdown();
				}
			}
		}
	}
}
