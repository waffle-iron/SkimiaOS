using NLog;
using NLog.Config;
using NLog.Targets;

namespace SkimiaOS.Core.Log
{
    public static class NLogProfile
    {
        public static string LogFormatConsole = "[${custom-callsite:opt=blockcenter:length=18:className=true:methodName=false:includeSourcePath=false:namespace=false}](${threadid}) ${message}";
        public static string LogInitFormatConsole = "[${processtime}][${custom-callsite:opt=blockcenter:length=18:className=true:methodName=false:includeSourcePath=false:namespace=false}](${threadid}) ${message}";
        public static string LogFormatFile = "[${level}] <${date:format=G}> ${message}";
        public static readonly string LogFilePath = "/logs/";

        public static void DefineLogProfile(bool activefileLog, bool activeconsoleLog)
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("custom-callsite", typeof(Log.CustomCallSiteLayoutRenderer));
            LoggingConfiguration loggingConfiguration = new LoggingConfiguration();
            ColoredConsoleTarget target = new ColoredConsoleTarget
            {
                Layout = NLogProfile.LogInitFormatConsole
            };
            FileTarget target2 = new FileTarget
            {
                FileName = "${basedir}" + NLogProfile.LogFilePath + "log_${date:format=dd-MM-yyyy}.txt",
                Layout = NLogProfile.LogFormatFile
            };
            FileTarget target3 = new FileTarget
            {
                FileName = "${basedir}" + NLogProfile.LogFilePath + "error_${date:format=dd-MM-yyyy}.txt",
                Layout = "-------------${level} at ${date:format=G}------------- ${newline} ${callsite} -> ${newline}\t${message} ${newline}-------------${level} at ${date:format=G}------------- ${newline}"
            };
            if (activefileLog)
            {
                loggingConfiguration.AddTarget("file", target2);
                loggingConfiguration.AddTarget("fileErrors", target3);
            }
            if (activeconsoleLog)
            {
                loggingConfiguration.AddTarget("console", target);
            }
            LogLevel debug = LogLevel.Debug;
            if (activeconsoleLog)
            {
                LoggingRule loggingRule = new LoggingRule("*", debug, target);
                loggingConfiguration.LoggingRules.Add(loggingRule);
            }
            if (activefileLog)
            {
                LoggingRule loggingRule = new LoggingRule("*", debug, target2);
                loggingRule.DisableLoggingForLevel(LogLevel.Fatal);
                loggingRule.DisableLoggingForLevel(LogLevel.Error);
                loggingConfiguration.LoggingRules.Add(loggingRule);
                LoggingRule item = new LoggingRule("*", LogLevel.Warn, target3);
                loggingConfiguration.LoggingRules.Add(item);
            }
            LogManager.Configuration = loggingConfiguration;
        }

        public static void EnableLogging()
        {
            LogManager.EnableLogging();
        }

        public static void DisableLogging()
        {
            LogManager.DisableLogging();
        }

        public static void disableInitialisation()
        {
            ColoredConsoleTarget target = LogManager.Configuration.FindTargetByName<ColoredConsoleTarget>("console");
            target.Layout = NLogProfile.LogFormatConsole;
        }
    }
}
