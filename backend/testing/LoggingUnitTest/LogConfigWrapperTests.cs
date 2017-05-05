using System.Collections.Generic;
using System.Linq;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using BaseLogging.Objects;
using BaseLogging.Objects.configurations;

namespace BaseLogging.UnitTest
{
    [TestClass]
    public class LogConfigWrapperTests
    {
        [TestMethod]
        public void NewDeserialTest()
        {
            var generalConfig = new Configuration
            {
                Database = BuildSQLConfiguration(),
                EmailConfiguration = BuildEmailConfiguration("DevEmail", "dev",
                    "nick.lawson@lawsoncs.com", "CommonLogging@lawsoncs.com", "mail.lawsoncs.com")
            };

            generalConfig.Log4NetConfigurations.AddRange(BuildLog4NetConfiguration());

            generalConfig.Systems = BuildSystemConfiguration();

            generalConfig.Serialize("loggingSettings.config");

            var newConfig = Configuration.Deserialize(File.ReadAllText("loggingSettings.config"));
        }
         
        [TestMethod] 
        public void SerializeToXMLTest()
        {
            var generalConfig = new Configuration
            {
                Database = BuildSQLConfiguration(),
                EmailConfiguration = BuildEmailConfiguration("DevEmail", "dev",
                    "nick.lawson@lawsoncs.com", "CommonLogging@lawsoncs.com", "mail.lawsoncs.com")
            };

            generalConfig.Log4NetConfigurations.AddRange(BuildLog4NetConfiguration());

            generalConfig.Systems = BuildSystemConfiguration(); 

            generalConfig.Serialize("loggingSettings.config");
             
            Assert.IsTrue(VerifySettingsFileExists());
        }

        [TestMethod]
        public void DeserializeFromXMLTestSQLConfiguration()
        {
            DatabaseConfiguration sqlConfig = LoggingSettings.Instance.Config.Database;
            
            Assert.IsNotNull(sqlConfig);
            Assert.AreEqual("SQLSaver", sqlConfig.LogName);
            Assert.AreEqual(SeverityLevel.Debug, sqlConfig.VerbosityLevel);
            Assert.AreEqual(true, sqlConfig.IsEnabled); 
        }

        [TestMethod]
        public void DeserializeFromXMLTestLog4NetConfiguration()
        {   
            Log4NetConfiguration log4NetConfigRoot = LoggingSettings.Instance.Config.Log4NetConfigurations.FirstOrDefault(x => x.LogName == LoggerInstanceConstants.MainLoggerInstance);
            Log4NetConfiguration log4NetConfigReq = LoggingSettings.Instance.Config.Log4NetConfigurations.FirstOrDefault(x => x.LogName == LoggerInstanceConstants.RequestLoggerInstance);
            Log4NetConfiguration log4NetConfigRep = LoggingSettings.Instance.Config.Log4NetConfigurations.FirstOrDefault(x => x.LogName == LoggerInstanceConstants.ReplyLoggerInstance);

            Assert.IsNotNull(log4NetConfigRoot);
            Assert.IsNotNull(log4NetConfigReq);
            Assert.IsNotNull(log4NetConfigRep);
             
            Assert.AreEqual(SeverityLevel.Debug, log4NetConfigRoot.VerbosityLevel);
            Assert.AreEqual(SeverityLevel.Debug, log4NetConfigReq.VerbosityLevel);
            Assert.AreEqual(SeverityLevel.Debug, log4NetConfigRep.VerbosityLevel);
            
            Assert.AreEqual(true, log4NetConfigRoot.IsEnabled);
            Assert.AreEqual(true, log4NetConfigReq.IsEnabled);
            Assert.AreEqual(true, log4NetConfigRep.IsEnabled);

        }

        [TestMethod]
        public void DeserializeFromXMLTestEmailConfiguration()
        { 
            List<string> emailToList = LoggingSettings.Instance.Config.EmailConfiguration.EmailTo.Split(';').ToList();
             
            Assert.IsTrue(emailToList.Contains("nick.lawson@lawsoncs.com"));
            Assert.AreEqual("dev", LoggingSettings.Instance.Config.EmailConfiguration.EmailName);

        } 

        [TestMethod]
        public void DeserializeConnectionStringTest()
        {
            var dbConfig = LoggingSettings.Instance.Config.Database;

            Assert.IsNotNull(dbConfig);
            Assert.AreEqual("Server=localhost\\SQLEXPRESS;Intial Catalog=logging;Integrated Security=True;", dbConfig.ConnectionString);

        }
        
        private static bool VerifySettingsFileExists()
        { 
            return File.Exists(Path.GetFullPath("loggingSettings.config"));
        }

        private static DatabaseConfiguration BuildSQLConfiguration()
        {
            var sqlConfig = new DatabaseConfiguration
            {
                LogName = "SQLSaver",
                VerbosityLevel = SeverityLevel.Debug,
                IsEnabled = true,
                ConnectionString = "Server=localhost\\SQLEXPRESS;Intial Catalog=logging;Integrated Security=True;",
                LogCacheTime = 30,
                LogCacheLimit = 10,
                MessageCacheTime = 30,
                MessageCacheLimit = 10,
                StackCacheLimit = 10,
                StackCacheTime = 30,
            };


            return sqlConfig;
        }

        private static SystemSaverConfiguration BuildSystemConfiguration()
        {
            var retVal = new SystemSaverConfiguration
            {
                IsEnabled = true,
                LogName = "SystemSaver",
                VerbosityLevel = SeverityLevel.Debug,
            };

            return retVal;
        }

        private static IEnumerable<Log4NetConfiguration> BuildLog4NetConfiguration()
        {
            var retVal = new List<Log4NetConfiguration>()
            {
                new Log4NetConfiguration
                {
                    LogName = "Main",
                    VerbosityLevel = SeverityLevel.Debug,
                    IsEnabled = true,
                    LogLocation = "C:\\Logs\\Testing\\"
                },
                
                new Log4NetConfiguration
                {
                    LogName = "Request",
                    VerbosityLevel = SeverityLevel.Debug,
                    IsEnabled = true,
                    LogLocation = "C:\\Logs\\Testing\\"
                },

                new Log4NetConfiguration
                {
                    LogName = "Reply",
                    VerbosityLevel = SeverityLevel.Debug,
                    IsEnabled = true,
                    LogLocation = "C:\\Logs\\Testing\\"
                },
            };

            return retVal;
        }

        private static EmailConfiguration BuildEmailConfiguration(string logname, string emailname, string emailto, string emailfrom, string emailsvr)
        {
            var email = new EmailConfiguration
            {
                LogName = logname,
                VerbosityLevel = SeverityLevel.Debug,
                IsEnabled = true,
                EmailName = emailname,
                EmailTo = emailto,
                EmailFrom = emailfrom,
                EmailServer = emailsvr,
            };


            return email;
        } 
    }
}
