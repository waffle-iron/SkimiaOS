using SkimiaOS.Core.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Server.BaseServer
{
    public class ConsoleBase
    {
        public static readonly string[] AsciiLogo = new string[]
        {
            " .d8888b. 888     d8b             d8b            .d88888b.  .d8888b. ",
            "d88P  Y88b888     Y8P             Y8P           d88P\" \"Y88bd88P  Y88b",
            "Y88b.     888                                   888     888Y88b.     ",
            " \"Y888b.  888  88888888888b.d88b. 888 8888b.    888     888 \"Y888b.  ",
            "    \"Y88b.888 .88P888888 \"888 \"88b888    \"88b   888     888    \"Y88b.",
            "      \"888888888K 888888  888  888888.d888888   888     888      \"888",
            "Y88b  d88P888 \"88b888888  888  888888888  888   Y88b. .d88PY88b  d88P ",
            " \"Y8888P\" 888  888888888  888  888888\"Y888888    \"Y88888P\"  \"Y8888P\" ",
        };

        protected string Cmd = "";
        protected readonly ConditionWaiter m_conditionWaiter;
        public bool EnteringCommand
        {
            get;
            set;
        }
        public bool AskingSomething
        {
            get;
            set;
        }
        protected ConsoleBase()
        {
            Func<bool> predicate = () => !this.AskingSomething && Console.KeyAvailable;
            this.m_conditionWaiter = new ConditionWaiter(predicate, -1, 20);
        }


        public static void SetTitle(string str)
        {
            Console.Title = str;
        }
        public static void DrawAsciiLogo()
        {
            ConsoleColor foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            string[] asciiLogo = ConsoleBase.AsciiLogo;
            for (int i = 0; i < asciiLogo.Length; i++)
            {
                string text = asciiLogo[i];
                int totalWidth = (Console.BufferWidth + text.Length) / 2;
                Console.WriteLine(text.PadLeft(totalWidth));
            }
            Console.ForegroundColor = foregroundColor;
        }


        public static void DrawLine(string str)
        {
            string line = "----------[ " + str + " ]----------";
            ConsoleColor foregroundColor = Console.ForegroundColor;
            ConsoleColor backgroundColor = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            int totalWidth = (Console.BufferWidth + line.Length) / 2;
            Console.WriteLine(line.PadLeft(totalWidth));


            Console.ForegroundColor = foregroundColor;
        }

        protected virtual void Process()
        {
        }

        public void Start()
		{
			Task.Factory.StartNew(new Action(this.Process));
		}
    }
}
