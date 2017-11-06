using System.Windows.Controls;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace SpExport.Util.Logger
{
    internal static class Logs
    {
        public static NLog.Logger Instance { get; private set; }

        static Logs()
        {
            LogManager.ReconfigExistingLoggers();
            // Set Log Target
            // https://nlog.codeplex.com/workitem/6272

            Instance = LogManager.GetCurrentClassLogger();
        }

        public static LoggingRule ReloadConsole(AsyncTargetWrapper wrapper)
        {
        //  if Configuration is null should be set nlog.config to copy output
            LogManager.Configuration.AddTarget("NameOfTargetIs_wrapper", wrapper);
         
            LoggingRule r1 = new LoggingRule("*", LogLevel.Debug, wrapper);
        
            LogManager.Configuration.LoggingRules.Add(r1);
      
            LogManager.Configuration.Reload();

            return r1;
        }

        public static bool Remove(LoggingRule loggingRule)
        {
            LogManager.Configuration.LoggingRules.Remove(loggingRule);
            LogManager.Configuration.Reload();

            return true;

        }

        public static AsyncTargetWrapper GetConsoleWrapper(string consoleTargetName,Target target )
        {
          var  wrapper = new AsyncTargetWrapper
            {
                Name = consoleTargetName,
                WrappedTarget = target
            };

            return wrapper;
        }
        public static Target GetConsoleTarget(string consoleTargetName, RichTextBox richTextBoxName,string formName)
        {
            var target = new WpfRichTextBoxTarget(richTextBoxName)
            {
                Name = consoleTargetName ,
                Layout = " ${counter} | ${time:} | ${level:uppercase=true:padding=-5} | ${message} ",
               // ControlName =richTextBoxName,
                FormName = formName,
                AutoScroll = true,
                MaxLines = 10,
                UseDefaultRowColoringRules = true
            };
            
            return target;
        }

    }
}
