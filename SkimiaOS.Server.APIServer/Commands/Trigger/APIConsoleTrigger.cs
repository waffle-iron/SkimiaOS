using SkimiaOS.Core.IO;
using SkimiaOS.Server.BaseServer.Commands;
using SkimiaOS.Server.BaseServer.Commands.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Server.APIServer.Commands.Trigger
{
    public class APIConsoleTrigger : TriggerBase
    {
        public override bool CanFormat
        {
            get
            {
                return false;
            }
        }
        public override ICommandsUser User
        {
            get
            {
                return APIServer.Instance.ConsoleInterface as APIConsole;
            }
        }
        public APIConsoleTrigger(StringStream args) : base(args, RoleEnum.Administrator)
        {
        }
        public APIConsoleTrigger(string args) : base(args, RoleEnum.Administrator)
        {
        }
        public override void Reply(string text)
        {
            Console.WriteLine(" " + text);
        }
        /*public override BaseClient GetSource()
        {
            return null;
        }*/
    }
}
