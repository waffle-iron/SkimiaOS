using SkimiaOS.Server.APIServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkimiaOS.CLI.ServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            APIServer server = new APIServer();

            //Debug from IDE
            if (Debugger.IsAttached)
            {
                server.Initialize();
                server.Start();

                GC.Collect();
                while (true)
                {
                    Thread.Sleep(5000);
                }
            }

            try
            {
                server.Initialize();
                server.Start();
                GC.Collect();
                while (true)
                {
                    Thread.Sleep(5000);
                }
            }
            catch (Exception e)
            {
                server.HandleCrashException(e);
            }
            finally
            {
                server.Shutdown();
            }
        }
    }
}
