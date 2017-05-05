using System;
using System.Linq;
using BaseLogging.Objects;

namespace BaseLogging.Data
{
    public class SystemSaver : ISaveLog, IFinalSaveLog
    {
        private readonly LoggingSettings _settings;

        private SystemSaver() { }

        public SystemSaver(LoggingSettings settings)
        {
            _settings = settings;
        } 

        #region Implementation of ISaveLog

        public void SaveLogs(Log l, string loggerInstance)
        {
            ConsoleLog(l, _settings, loggerInstance);
        }

        public void Flush()
        {
        }

        #endregion

        #region Implementation of IFinalSaveLog

        public void EmergencySaveLog(Log l)
        {
            try
            {
                EmergencySaveLogProxy(l);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        
        public static void EmergencySaveLogProxy(Log l)
        { 
            if (l == null || l.LogMessages == null || !l.LogMessages.Any()) return;

            if (l.LogMessages[0].LogMessageText != null)
            {
                Console.WriteLine(l.LogMessages[0].LogMessageText.MessageText);
            }

            if (l.LogMessages[0].LogStackTrace != null)
            {
                Console.WriteLine(l.LogMessages[0].LogStackTrace.StackTraceText);
            }
        }

        #endregion

        private static void ConsoleLog(Log l, LoggingSettings settings, string loggerInstance = null)
        {
            Console.WriteLine(new string('*', 50));
            Console.WriteLine("* Calling Method {0}", l.CallingMethod);
            Console.WriteLine("* Severity {0}", l.Severity);

            if (l.LogMessages!=null && l.LogMessages.Any())
                Console.WriteLine("* Message {0}", l.LogMessages[0].LogMessageText.MessageText);


            Console.WriteLine(new string('*', 50));
            Console.WriteLine("* Exceptions");
            Console.WriteLine(new string('*', 50));

            if (l.LogMessages != null)
            {
                foreach (LogMessage lLogMessage in l.LogMessages)
                {
                    Console.WriteLine("* Message {0}", lLogMessage.LogMessageText.MessageText);
                    Console.WriteLine(new string('-', 50));
                    Console.WriteLine("* Stack: ");

                    if (lLogMessage.LogStackTrace != null &&
                        !string.IsNullOrWhiteSpace(lLogMessage.LogStackTrace.StackTraceText))
                    {
                        string[] splitStack = lLogMessage.LogStackTrace.StackTraceText.Split(new string[] {"at"},
                            StringSplitOptions.None);

                        foreach (var s in splitStack)
                        {
                            Console.WriteLine("AT {0}", s);
                            Console.WriteLine("*-----");
                        }
                    }
                }
            }

            Console.WriteLine(new string('*', 50));
            Console.WriteLine(new string('*', 50));
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
        }

        private static void EventLog(Log l)
        {
            //ToDo write event to event viewer
        }
    }
}
