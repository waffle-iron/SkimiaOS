using NLog;
using SkimiaOS.Core.Log;
using SkimiaOS.Core.Threading;
using SkimiaOS.Core.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using SkimiaOS.Core.Reflection;
using SkimiaOS.Server.BaseServer.Exceptions;
using SkimiaOS.Server.BaseServer.Plugins;
using System.Threading;
using SkimiaOS.Server.BaseServer.Commands;
using SkimiaOS.Core.Mathematics;
using SkimiaOS.Server.BaseServer.Initialization;
using SkimiaOS.Core.Messages;
using SkimiaOS.Server.BaseServer.Messages;

namespace SkimiaOS.Server.BaseServer
{
    public abstract class ServerBase
    {
        internal static ServerBase InstanceAsBase;
        [Configurable]
        public static int IOTaskInterval = 50;
        [Configurable]
        public static bool ScheduledAutomaticShutdown = true;
        [Configurable]
        public static int AutomaticShutdownTimer = 360;
        [Configurable]
        public static string CommandsInfoFilePath = "./config/commands.xml";

        protected Dictionary<string, Assembly> LoadedAssemblies;
        protected Logger logger;

        public string ConfigFilePath
        {
            get;
            protected set;
        }
        public string SchemaFilePath
        {
            get;
            protected set;
        }
        public Config Config
        {
            get;
            protected set;
        }
        public DispatcherTask DispatcherTask
        {
            get;
            private set;
        }
        public ConsoleBase ConsoleInterface
        {
            get;
            protected set;
        }

        public CommandManager CommandManager
        {
            get;
            protected set;
        }

        public SelfRunningTaskPool IOTaskPool
        {
            get;
            protected set;
        }

        public InitializationManager InitializationManager
        {
            get;
            protected set;
        }

        public PluginManager PluginManager
        {
            get;
            protected set;
        }


        public bool Running
        {
            get;
            protected set;
        }
        public DateTime StartTime
        {
            get;
            protected set;
        }
        public TimeSpan UpTime
        {
            get
            {
                return DateTime.Now - this.StartTime;
            }
        }

        public bool Initializing
        {
            get;
            protected set;
        }
        public bool IsInitialized
        {
            get;
            protected set;
        }

        public bool IsShutdownScheduled
        {
            get;
            protected set;
        }
        public DateTime ScheduledShutdownDate
        {
            get;
            protected set;
        }
        public string ScheduledShutdownReason
        {
            get;
            protected set;
        }

        public System.TimeSpan? LastAnnouncedTime
        {
            get;
            protected set;
        }


        protected ServerBase(string configFile, string schemaFile)
        {
            this.ConfigFilePath = configFile;
            this.SchemaFilePath = schemaFile;
        }

        public virtual void Initialize()
        {
            ServerBase.InstanceAsBase = this;
            this.Initializing = true;

            //Initializing NLog
            NLogProfile.DefineLogProfile(true, true);
            NLogProfile.EnableLogging();
            this.logger = LogManager.GetCurrentClassLogger();

            if (!Debugger.IsAttached)
            {
                //Register Application Crash Handlers
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.OnUnhandledException);
                TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(this.OnUnobservedTaskException);
                Contract.ContractFailed += new EventHandler<ContractFailedEventArgs>(this.OnContractFailed);
            }
            else
            {
                this.logger.Warn("Exceptions not handled cause Debugger is attatched");
                Console.WriteLine();
            }

            //Preload All referenced assemblies and alls her references
            ServerBase.PreLoadReferences(Assembly.GetCallingAssembly());

            //keep a list of all loaded assemblies
            this.LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToDictionary((Assembly entry) => entry.GetName().Name);
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(this.OnAssemblyLoad);

            if (Environment.GetCommandLineArgs().Contains("-config"))
            {
                //regenerate config files if asked
                this.UpdateConfigFiles();
            }

            //Draw the logo on console
            ConsoleBase.DrawAsciiLogo();
            Console.WriteLine();

            //Set Mode of Garbage collection
            ServerBase.InitializeGarbageCollector();

            this.logger.Info("Initializing Configuration...");

            this.Config = new Config(this.ConfigFilePath);
            this.Config.AddAssemblies(this.LoadedAssemblies.Values.ToArray<Assembly>());
            if (!File.Exists(this.ConfigFilePath))
            {
                this.Config.Create(false);
                this.logger.Info("Config file created");
            }
            else
            {
                this.Config.Load();
            }


            this.logger.Info("Register Assemblies in Dispatcher");
            foreach (Assembly assembly in this.LoadedAssemblies.Values.ToArray<Assembly>())
            {
                MessageDispatcher.RegisterSharedAssembly(assembly);
            }


            this.logger.Info("Initialize Task Pool");
            this.IOTaskPool = new SelfRunningTaskPool(ServerBase.IOTaskInterval, "IO Task Pool");

            this.logger.Info("Register Commands...");
            this.CommandManager = Singleton<CommandManager>.Instance;
            this.CommandManager.RegisterAll(Assembly.GetExecutingAssembly());

            this.logger.Info("Starting Dispatcher...");
            DispatcherTask = new DispatcherTask(new MessageDispatcher());
            DispatcherTask.Start(); // we have to start it now to dispatch the initialization msg

            this.logger.Info("Register Initialization...");
            this.InitializationManager = Singleton<InitializationManager>.Instance;
            this.InitializationManager.AddAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            this.PluginManager = Singleton<PluginManager>.Instance;
            this.PluginManager.PluginAdded += new PluginManager.PluginContextHandler(this.OnPluginAdded);
            this.PluginManager.PluginRemoved += new PluginManager.PluginContextHandler(this.OnPluginRemoved);
            this.logger.Info("Loading Plugins...");
            Singleton<PluginManager>.Instance.LoadAllPlugins();

            this.CommandManager.LoadOrCreateCommandsInfo(ServerBase.CommandsInfoFilePath);

            var msg = new BaseServerInitializationMessage();
            DispatcherTask.Dispatcher.Enqueue(msg, this);

            msg.Wait();
        }

        public virtual void UpdateConfigFiles()
        {
            this.logger.Info("Recreate server config file ...");
            if (File.Exists(this.ConfigFilePath))
            {
                this.logger.Info("Update {0} file", this.ConfigFilePath);
                this.Config = new Config(this.ConfigFilePath);
                this.Config.AddAssemblies(this.LoadedAssemblies.Values.ToArray<Assembly>());
                this.Config.Load();
                this.Config.Create(true);
            }
            else
            {
                this.logger.Info("Create {0} file", this.ConfigFilePath);
                this.Config = new Config(this.ConfigFilePath);
                this.Config.AddAssemblies(this.LoadedAssemblies.Values.ToArray<Assembly>());
                this.Config.Create(false);
            }
            this.logger.Info("Recreate plugins config files ...", this.ConfigFilePath);
            this.PluginManager = Singleton<PluginManager>.Instance;
            Singleton<PluginManager>.Instance.LoadAllPlugins();
            foreach (PluginBase current in (
                from entry in this.PluginManager.GetPlugins()
                select entry.Plugin).OfType<PluginBase>())
            {
                if (current.UseConfig && current.AllowConfigUpdate)
                {
                    bool flag;
                    if (!(flag = File.Exists(current.GetConfigPath())))
                    {
                        this.logger.Info<string, string>("Create '{0}' config file => '{1}'", current.Name, Path.GetFileName(current.GetConfigPath()));
                    }
                    current.LoadConfig();
                    if (flag)
                    {
                        this.logger.Info<string, string>("Update '{0}' config file => '{1}'", current.Name, Path.GetFileName(current.GetConfigPath()));
                        current.Config.Create(true);
                    }
                }
            }
            this.logger.Info("All config files were correctly updated/created ! Shutdown ...");
            Thread.Sleep(TimeSpan.FromSeconds(2.0));
            Environment.Exit(0);
        }

        private static void PreLoadReferences(Assembly executingAssembly)
        {
            foreach (Assembly current in
                from assemblyName in executingAssembly.GetReferencedAssemblies()
                where AppDomain.CurrentDomain.GetAssemblies().Count((Assembly entry) => entry.GetName().FullName == assemblyName.FullName) <= 0
                select Assembly.Load(assemblyName))
            {
                ServerBase.PreLoadReferences(current);
            }
        }

        protected virtual void OnPluginRemoved(PluginContext plugincontext)
        {
            this.logger.Info("Plugin Unloaded : {0}", plugincontext.Plugin.GetDefaultDescription());
            DispatcherTask.Dispatcher.Enqueue(new PluginRemovedMessage(plugincontext), this);
        }
        protected virtual void OnPluginAdded(PluginContext plugincontext)
        {
            this.logger.Info("Plugin Loaded : {0}", plugincontext.Plugin.GetDefaultDescription());
            DispatcherTask.Dispatcher.Enqueue(new PluginAddedMessage(plugincontext), this);
        }

        private static void InitializeGarbageCollector()
        {
            GCSettings.LatencyMode = (GCSettings.IsServerGC ? GCLatencyMode.Batch : GCLatencyMode.Interactive);
        }
        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs e)
        {
            string name = e.LoadedAssembly.GetName().Name;
            if (!this.LoadedAssemblies.ContainsKey(name))
            {
                this.LoadedAssemblies.Add(name, e.LoadedAssembly);
            }
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            this.HandleCrashException(e.Exception);
            e.SetObserved();
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.HandleCrashException((Exception)e.ExceptionObject);
            if (e.IsTerminating)
            {
                this.Shutdown();
            }
        }
        private void OnContractFailed(object sender, ContractFailedEventArgs e)
        {
            this.logger.Fatal("Contract failed : {0}", e.Condition);
            if (e.OriginalException != null)
            {
                this.HandleCrashException(e.OriginalException);
            }
            else
            {
                this.logger.Fatal(string.Format(" Stack Trace:\r\n{0}", Environment.StackTrace));
            }
            e.SetHandled();
        }
        public void HandleCrashException(Exception e)
        {
            Singleton<ExceptionManager>.Instance.RegisterException(e);
            this.logger.Fatal(string.Format(" Crash Exception : {0}\r\n", e.Message) + string.Format(" Source: {0} -> {1}\r\n", e.Source, e.TargetSite) + string.Format(" Stack Trace:\r\n{0}", e.StackTrace));
            if (e.InnerException != null)
            {
                this.HandleCrashException(e.InnerException);
            }
        }

        public virtual void Start()
        {
            this.Running = true;
            this.Initializing = false;

            if(ServerBase.ScheduledAutomaticShutdown)
                this.logger.Info(String.Format("Automatic Shutdown : Server shutdown in {0} minutes", ServerBase.AutomaticShutdownTimer));
            this.IOTaskPool.CallPeriodically((int)TimeSpan.FromSeconds(5.0).TotalMilliseconds, new Action(this.CheckScheduledShutdown));
        }

        protected virtual void OnShutdown()
        {
            if(this.IOTaskPool != null)
                this.IOTaskPool.Stop(false);
        }
        public virtual void ScheduleShutdown(TimeSpan timeBeforeShuttingDown, string reason)
        {
            this.ScheduledShutdownReason = reason;
            this.ScheduleShutdown(timeBeforeShuttingDown);
        }
        public virtual void ScheduleShutdown(TimeSpan timeBeforeShuttingDown)
        {
            this.IsShutdownScheduled = true;
            this.ScheduledShutdownDate = DateTime.Now + timeBeforeShuttingDown;
        }
        public virtual void CancelScheduledShutdown()
        {
            this.IsShutdownScheduled = false;
            this.ScheduledShutdownDate = DateTime.MaxValue;
            this.ScheduledShutdownReason = null;
        }
        protected virtual void CheckScheduledShutdown()
        {
            System.TimeSpan timeSpan = System.TimeSpan.FromMinutes((double)ServerBase.AutomaticShutdownTimer) - this.UpTime;
            bool automatic = true;
            if (this.IsShutdownScheduled && timeSpan > this.ScheduledShutdownDate - System.DateTime.Now)
            {
                timeSpan = this.ScheduledShutdownDate - System.DateTime.Now;
                automatic = false;
            }
            if (timeSpan < System.TimeSpan.FromMinutes(360.0))
            {
                string str = automatic ? "Automatic reboot in " : "Reboot in ";
                if (timeSpan > System.TimeSpan.FromMinutes(60.0))
                    str += "{0:%h}h {0:mm}m {0:ss}s";
                else
                    str += "{0:mm}m {0:ss}s";

                str = String.Format(str, timeSpan);
                if (!automatic && !string.IsNullOrEmpty(this.ScheduledShutdownReason))
                {
                    str = str + " : " + this.ScheduledShutdownReason;
                }

                System.TimeSpan? timeSpan2 = !this.LastAnnouncedTime.HasValue ? new System.TimeSpan(TimeSpan.MaxValue.Ticks) : (this.LastAnnouncedTime - timeSpan);

                if (timeSpan > System.TimeSpan.FromMinutes(30.0) && timeSpan2 >= System.TimeSpan.FromMinutes(30.0))
                {
                    this.AnnounceTimeBeforeShutdown(str, System.TimeSpan.FromMinutes(timeSpan.TotalMinutes.RoundToNearest(10.0)), automatic);
                }

                if (timeSpan > System.TimeSpan.FromMinutes(10.0) && timeSpan <= System.TimeSpan.FromMinutes(30.0) && timeSpan2 >= System.TimeSpan.FromMinutes(5.0))
                {
                    this.AnnounceTimeBeforeShutdown(str, System.TimeSpan.FromMinutes(timeSpan.TotalMinutes.RoundToNearest(5.0)), automatic);
                }
                if (timeSpan > System.TimeSpan.FromMinutes(5.0) && timeSpan <= System.TimeSpan.FromMinutes(10.0) && timeSpan2 >= System.TimeSpan.FromMinutes(1.0))
                {
                    this.AnnounceTimeBeforeShutdown(str, System.TimeSpan.FromMinutes(timeSpan.TotalMinutes), automatic);
                }
                if (timeSpan > System.TimeSpan.FromMinutes(1.0) && timeSpan <= System.TimeSpan.FromMinutes(5.0) && timeSpan2 >= System.TimeSpan.FromSeconds(30.0))
                {
                    this.AnnounceTimeBeforeShutdown(str, new System.TimeSpan(0, 0, 0, (int)timeSpan.TotalSeconds.RoundToNearest(30.0)), automatic);
                }
                if (timeSpan > System.TimeSpan.FromSeconds(10.0) && timeSpan <= System.TimeSpan.FromMinutes(1.0) && timeSpan2 >= System.TimeSpan.FromSeconds(10.0))
                {
                    this.AnnounceTimeBeforeShutdown(str, new System.TimeSpan(0, 0, 0, (int)timeSpan.TotalSeconds.RoundToNearest(10.0)), automatic);
                }
                if (timeSpan <= System.TimeSpan.FromSeconds(10.0) && timeSpan > System.TimeSpan.Zero)
                {
                    this.AnnounceTimeBeforeShutdown(str, System.TimeSpan.FromSeconds(timeSpan.Seconds.RoundToNearest(5)), automatic);
                }
            }

            if ((ServerBase.ScheduledAutomaticShutdown && this.UpTime.TotalMinutes > (double)ServerBase.AutomaticShutdownTimer) || (this.IsShutdownScheduled && this.ScheduledShutdownDate <= DateTime.Now))
            {
                this.Shutdown();
            }
        }
        protected virtual void AnnounceTimeBeforeShutdown(string str, System.TimeSpan time, bool automatic)
        {
            ConsoleColor foregroundColor = Console.ForegroundColor;
            ConsoleColor backgroundColor = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            int totalWidth = (Console.BufferWidth + str.Length) / 2;
            Console.WriteLine(str.PadLeft(totalWidth));


            Console.ForegroundColor = foregroundColor;
            this.LastAnnouncedTime = new System.TimeSpan?(time);
        }
        public void Shutdown()
        {
            bool flag = false;
            try
            {
                Monitor.Enter(this, ref flag);
                this.Running = false;
                this.OnShutdown();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Console.WriteLine("Application is now terminated. Wait " + Definitions.ExitWaitTime + " seconds to exit ... or press any key to cancel");
                if (ConditionWaiter.WaitFor(() => Console.KeyAvailable, Definitions.ExitWaitTime * 1000, 20))
                {
                    Console.ReadKey(true);
                    Thread.Sleep(500);
                    Console.WriteLine("Press now a key to exit...");
                    Console.ReadKey(true);
                }
                Environment.Exit(0);
            }
            finally
            {
                if (flag)
                {
                    Monitor.Exit(this);
                }
            }
        }
    }

    public abstract class ServerBase<T> : ServerBase where T : class
    {
        public static T Instance;
        protected ServerBase(string configFile, string schemaFile) : base(configFile, schemaFile)
        {
        }
        public override void Initialize()
        {
            ServerBase<T>.Instance = (this as T);
            base.Initialize();
        }

    }
}