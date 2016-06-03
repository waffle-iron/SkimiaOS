using SkimiaOS.Core.Config;
using SkimiaOS.Core.IO;
using SkimiaOS.Server.APIServer.Commands.Trigger;
using SkimiaOS.Server.BaseServer;
using SkimiaOS.Server.BaseServer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Server.APIServer
{
    public class APIConsole : ConsoleBase, ICommandsUser
    {
        [Configurable]
        public static string CommandPreffix = "";

        [Configurable]
        public static string ConsolePrefix = "SkimiaOS:# ";

        private List<KeyValuePair<string, Exception>> m_commandsError = new List<KeyValuePair<string, Exception>>();
        public List<KeyValuePair<string, Exception>> CommandsErrors
        {
            get
            {
                return this.m_commandsError;
            }
        }
        public APIConsole()
        {
            this.m_conditionWaiter.Success += new EventHandler(this.OnConsoleKeyPressed);
        }
        protected void DrawLinePrefix()
        {
            ConsoleColor foregroundColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(ConsolePrefix);

            Console.ForegroundColor = foregroundColor;
        }
        protected override void Process()
        {
            //this.DrawLinePrefix();
            this.m_conditionWaiter.Start();
        }
        private void OnConsoleKeyPressed(object sender, EventArgs e)
        {
            base.EnteringCommand = true;
            if (!APIServer.Instance.Running)
            {
                base.EnteringCommand = false;
            }
            else
            {
                try
                {
                    this.Cmd = Console.ReadLine();
                }
                catch (Exception)
                {
                    base.EnteringCommand = false;
                    return;
                }
                if (this.Cmd == null || !APIServer.Instance.Running)
                {
                    base.EnteringCommand = false;
                }
                else
                {
                    base.EnteringCommand = false;
                    lock (Console.Out)
                    {
                        try
                        {
                            if (this.Cmd.StartsWith(APIConsole.CommandPreffix))
                            {
                                this.Cmd = this.Cmd.Substring(APIConsole.CommandPreffix.Length);
                                APIServer.Instance.CommandManager.HandleCommand(new APIConsoleTrigger(new StringStream(this.Cmd)));
                            }
                        }
                        finally
                        {
                            //this.DrawLinePrefix();
                            this.m_conditionWaiter.Start();
                        }
                    }
                }
            }
        }
    }
}
