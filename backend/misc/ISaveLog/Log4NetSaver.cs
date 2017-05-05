using System;
using System.Configuration;
using System.IO;
using System.Linq;
using BaseLogging.Objects;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace BaseLogging.Data
{
    public class Log4NetSaveLog : ISaveLog
    {  
        private readonly LoggingSettings _settings;

        public Log4NetSaveLog(LoggingSettings settings)
        {
            _settings = settings;

            foreach (Log4NetConfiguration log4NetConfiguration in _settings.Config.Log4NetConfigurations)
            {
                SetupLogNetLogger(log4NetConfiguration.LogName, log4NetConfiguration.LogLocation, log4NetConfiguration.VerbosityLevel);
            }
        }

        public void SetupLogNetLogger(string loggerName, string logLocation, SeverityLevel sl)
        {
            ILog log = LogManager.GetLogger(loggerName);
            var logger = (Logger)log.Logger;

            //Add the default log appender if none exist
            if (logger.Appenders.Count != 0) return; 

            //If the directory doesn't exist then create it
            if (!Directory.Exists(logLocation))
                Directory.CreateDirectory(logLocation); 

            var patternLayout = new PatternLayout
            {
                ConversionPattern = "\a%date [%thread] %-5level %logger - %message%newline"
            };

            //Create the rolling file appender
            var appender = new RollingFileAppender
            {
                Name = "RollingFileAppener" + loggerName,
                AppendToFile = true,
                File = logLocation,
                Layout = patternLayout, 
                MaximumFileSize = "100MB",
                RollingStyle = RollingFileAppender.RollingMode.Date,
                DatePattern = "'" + loggerName + "-'yyyy-MM-dd'.log'",
                StaticLogFileName = false,
                ImmediateFlush = true, 
            };

            switch (sl)
            { 
                case SeverityLevel.Debug: 
                    appender.Threshold= Level.Debug;
                    break;
                case SeverityLevel.Warn:
                    appender.Threshold = Level.Warn;
                    break;
                case SeverityLevel.Info:
                    appender.Threshold = Level.Info;
                    break;
                case SeverityLevel.Error:
                    appender.Threshold = Level.Error;
                    break;
                case SeverityLevel.Fatal:
                    appender.Threshold = Level.Fatal;
                    break;
                default:
                    appender.Threshold = Level.Debug;
                    break;
            }

            //Configure the layout of the trace message write
            var layout = new PatternLayout()
            {
                ConversionPattern = "%date [%thread] %-5level %message%newline"
            };

            appender.Layout = layout;
            layout.ActivateOptions();

            //Let log4net configure itself based on the values provided
            appender.ActivateOptions();
            log4net.Config.BasicConfigurator.Configure(appender);
            logger.Additivity = false;

            logger.AddAppender(appender);

            //if(LoggerInstanceConstants.MainLoggerInstance == loggerName)
            //    ((log4net.Repository.Hierarchy.Hierarchy)logger.Repository).Root.AddAppender(appender);
            
        }

        public void SaveLogs(Log l, string loggerInstance)
        {
            Log4NetConfiguration config = _settings.Config.Log4NetConfigurations.FirstOrDefault(c => c.LogName == loggerInstance);

            if (config == null)
            {
                throw new ConfigurationErrorsException(string.Format("Log4Net configuration for {0} not found",loggerInstance));
            }

            if (!config.IsEnabled)
            {
                return;
            }

            if (l.Severity < config.VerbosityLevel)
            {
                return;
            }

            ILog tempLogger = LogManager.GetLogger(loggerInstance);

            if (tempLogger == null)
            {
                throw new NullReferenceException(string.Format("log4net instance not found for {0} logger instance", loggerInstance));
            } 

            switch (l.Severity)
            {
                case SeverityLevel.Debug:
                    tempLogger.Debug(l.ToString());
                    break;
                case SeverityLevel.Warn:
                    tempLogger.Warn(l.ToString());
                    break;
                case SeverityLevel.Info:
                    tempLogger.Info(l.ToString());
                    break;
                case SeverityLevel.Error:
                    tempLogger.Error(l.ToString());
                    break;
                case SeverityLevel.Fatal:
                    tempLogger.Fatal(l.ToString());
                    break;
                default:
                    tempLogger.Debug(l.ToString());
                    break;
            }
        }

        public void Flush()
        {
            ILoggerRepository rep = LogManager.GetRepository();
            foreach (IAppender appender in rep.GetAppenders())
            {
                var buffered = appender as BufferingAppenderSkeleton;
                if (buffered != null)
                {
                    buffered.Flush();
                }
            }
        }
    }
}
