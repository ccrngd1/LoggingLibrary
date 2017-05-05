using System;
using System.IO; 

namespace BaseLogging.Objects
{
    public class LoggingSettings
    {
        private static volatile LoggingSettings _instance;

        public readonly Configuration Config;

        private static readonly object SyncRoot = new object();

        public int DuplicateCachePurgeSeconds = 60*5;

        private LoggingSettings()
        {
            var filePath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "\\loggingSettings.config");
            //Config = filePath.Deserialize<Configuration>();

            Config= Configuration.Deserialize(File.ReadAllText(filePath));

            if (Config.EmailConfiguration.EmailName.ToLower() == "dev")
            {
                Config.EmailConfiguration.IsEnabled = false;
            }
        }

        public static LoggingSettings Instance
        {
            get
            {
                if (_instance != null) return _instance;

                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new LoggingSettings();
                }

                return _instance;
            }
        }
    }
}
