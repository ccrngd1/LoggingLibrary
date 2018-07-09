using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using System.Xml.Serialization;
using BaseLogging.Objects.configurations;

namespace BaseLogging.Objects
{
    public class Configuration
    {
        [XmlElement(ElementName = "Log4NetConfiguration")]
        public List<Log4NetConfiguration> Log4NetConfigurations { get; set; } 

        [XmlElement(ElementName = "Email")]
        public EmailConfiguration EmailConfiguration { get; set; }

        [XmlElement(ElementName = "Database")]
        public DatabaseConfiguration Database { get; set; }

        [XmlElement(ElementName = "System")]
        public SystemSaverConfiguration Systems { get; set; } 

	    [XmlElement(ElementName = "Splunk")]
	    public SplunkConfiguration Splunk { get; set; }

        public Configuration()
        { 
            Log4NetConfigurations = new List<Log4NetConfiguration>(); 
        }

        private void SetDefault()
        {
            SetDefaultLog4Net("Main");
            SetDefaultLog4Net("Reply");
            SetDefaultLog4Net("Request");

            SetDefaultDatabase();

            SetDefaultEmail();

            SetDefaultSysetm();
        }

        private void SetDefaultLog4Net(string loggerInst)
        {
            Log4NetConfigurations.Add(new Log4NetConfiguration
            {
                IsEnabled = true,
                LogLocation = @"C:\Logs\" + AppDomain.CurrentDomain.FriendlyName + "\\" ,
                LogName = loggerInst,
                VerbosityLevel = SeverityLevel.Error,
        });
        }

        private void SetDefaultDatabase()
        {
            Database = new DatabaseConfiguration
            {
                IsEnabled = false,
            };
        }

        private void SetDefaultSysetm()
        {
            Systems = new SystemSaverConfiguration
            {
                IsEnabled = true,
                LogName = "SystemSaver",
                VerbosityLevel = SeverityLevel.Debug,
            };
        }

        private void SetDefaultEmail()
        {
            bool isEn = true;

            string env=string.Empty;
            switch (Environment.MachineName.ToLower()[3])
            {
                case 'd':
                    env = "dev";
                    break;
                case 'q':
                    env = "qa";
                    break;
                case 'p':
                    env = "prod";
                    break;
                default:
                    isEn = false;
                    break;
            }

            string emailServer = env + " mail.lawsoncs.com ";

            EmailConfiguration = new EmailConfiguration
            {
                IsEnabled = isEn,
                VerbosityLevel = SeverityLevel.Error,
                LogName = env + "Email",
                EmailFrom = "Errors@lawsoncs.com",
                EmailName = env,
                EmailServer = emailServer,
                EmailTo = "nick.lawson@lawsoncs.com",
            };
        }

        private static SeverityLevel TediousEnumParse(XmlElement verbCheck)
        {
            var sl = SeverityLevel.Error;

            if (verbCheck == null) return sl;
            
            switch (verbCheck.InnerText.ToLower())
            {
                case "debug":
                    sl = SeverityLevel.Debug;
                    break;
                case "warn":
                    sl = SeverityLevel.Warn;
                    break;
                case "info":
                    sl = SeverityLevel.Info;
                    break;
                case "error":
                    sl = SeverityLevel.Error;
                    break;
                case "fatal":
                    sl = SeverityLevel.Fatal;
                    break;
                default:
                    try
                    {
                        Enum.TryParse(verbCheck.InnerText, out sl);
                    }
                    catch
                    {
                        sl = SeverityLevel.Error;
                    }
                    break;
            }

            return sl;
        }

        private static EmailConfiguration SetupEmail(XmlNode confSection)
        {
            if (confSection == null) return null;

            string env;
            switch (Environment.MachineName.ToLower()[3])
            {
                case 'd':
                    env = "dev";
                    break;
                case 'q':
                    env = "qa";
                    break;
                case 'p':
                    env = "prod";
                    break;
                default:
#if DEBUG
                    env = "dev";
                    break;
#else
                    return null; //if it isn't one of these, don't try to set the email up, it will fail
#endif
            }

            string emailServer = env + " mail.lawsoncs.com";
            string from = "CommonLogging@lawsoncs.com";
            string to = "error@lawsoncs.com";
            SeverityLevel sl = SeverityLevel.Error;
            bool isEnabled = true;

            XmlElement enableCheck = confSection["IsEnabled"];
            if (enableCheck != null)
            {
                if (enableCheck.InnerText.ToLower() == "false")
                {
                    isEnabled = false;
                }
            }

            XmlElement verbCheck = confSection["VerbosityLevel"];
            if (verbCheck != null)
            {
                sl = TediousEnumParse(verbCheck);
            }

            XmlElement toCheck = confSection["EmailTo"];
            if (toCheck != null)
            {
                to = toCheck.InnerText;
            }

            XmlElement fromCheck = confSection["EmailFrom"];
            if (fromCheck != null)
            {
                from = fromCheck.InnerText;
            }

            XmlElement svrCheck = confSection["EmailServer"];
            if (svrCheck != null)
            {
                emailServer = svrCheck.InnerText;
            }

            var retVal = new EmailConfiguration
            {
                LogName = env+"Email",
                EmailFrom = from,
                EmailName = env,
                EmailServer = emailServer,
                EmailTo = to,
                IsEnabled = isEnabled,
                VerbosityLevel = sl,
            };

            return retVal;
        }

        private static DatabaseConfiguration SetUpDatabase(XmlNode confSection)
        {
            if (confSection == null) return null;

            bool isEnabled = false;
            SeverityLevel sl = SeverityLevel.Error;
            string connString=string.Empty;
            int stackLimit=0;
            int stackTime = 0;
            int msgLimit = 0;
            int msgTime = 0;
            int logTime = 0;
            int logLimit = 0;

            XmlElement verbCheck = confSection["VerbosityLevel"];
            if (verbCheck != null)
            {
                sl = TediousEnumParse(verbCheck);
            } 

            XmlElement enableCheck = confSection["IsEnabled"];
            if (enableCheck != null)
            {
                if (enableCheck.InnerText.ToLower() == "true")
                    isEnabled = true;
            }

            XmlElement connCheck = confSection["ConnectionString"];
            if (connCheck != null)
            {
                connString = connCheck.InnerText;
            }

            XmlElement slCheck = confSection["StackCacheLimit"];
            if (slCheck != null)
            {
                if (!int.TryParse(slCheck.InnerText, out stackLimit))
                    isEnabled = false;
            }
            else { isEnabled = false; }

            XmlElement stCheck = confSection["StackCacheTime"];
            if (stCheck != null)
            {
                if (!int.TryParse(stCheck.InnerText, out stackTime))
                    isEnabled = false;
            }
            else { isEnabled = false; }

            XmlElement mtCheck = confSection["MessageCacheTime"];
            if (mtCheck != null)
            {
                if (!int.TryParse(mtCheck.InnerText, out msgTime))
                    isEnabled = false;
            }
            else { isEnabled = false; }

            XmlElement mlCheck = confSection["MessageCacheLimit"];
            if (mlCheck != null)
            {
                if (!int.TryParse(mlCheck.InnerText, out msgLimit))
                    isEnabled = false;
            }
            else { isEnabled = false; }

            XmlElement ltCheck = confSection["LogCacheTime"];
            if (ltCheck != null)
            {
                if (!int.TryParse(ltCheck.InnerText, out logTime))
                    isEnabled = false;
            }
            else { isEnabled = false; }

            XmlElement llCheck = confSection["LogCacheLimit"];
            if (llCheck != null)
            {
                if (!int.TryParse(llCheck.InnerText, out logLimit))
                    isEnabled = false;
            }
            else { isEnabled = false; }

            if (string.IsNullOrWhiteSpace(connString))
                isEnabled = false;

            var retVal = new DatabaseConfiguration
            {
                IsEnabled = isEnabled,
                ConnectionString = connString,
                VerbosityLevel = sl,
                LogName = "SQLSaver",
                MessageCacheTime = msgTime,
                LogCacheTime = logTime,
                StackCacheTime = stackTime,
                LogCacheLimit = logLimit,
                StackCacheLimit = stackLimit,
                MessageCacheLimit = msgLimit,
            };

            return retVal;
        }

        private static SystemSaverConfiguration SetUpSystem(XmlNode confSection)
        {
            if (confSection == null) return null;

            var isEnable = true;
            var sl = SeverityLevel.Debug;
            
            XmlElement verbCheck = confSection["VerbosityLevel"];
            if (verbCheck != null)
            {
                sl = TediousEnumParse(verbCheck);
            }
            
            XmlElement enCheck = confSection["IsEnabled"];
            if (enCheck != null)
            {
                if (enCheck.InnerText.ToLower() == "false")
                    isEnable = false;
            }

            var retVal = new SystemSaverConfiguration
            {
                IsEnabled = isEnable,
                VerbosityLevel = sl,
                LogName = "SystemSaver",
            };

            return retVal;
        }

        private static Log4NetConfiguration SetUpL4N(XmlNode confSection) {

            if (confSection ==null || confSection["LogName"] == null) return null;

            var isEnabled = false;

            //just need to set the correct sentinel bit
            //once we have read in the file, anything that is default needs to be manually generated for the basic ones
            switch (confSection["LogName"].InnerText.ToLower())
            {
                case "reqeust":
                    isEnabled = true; 
                    break;
                case "reply":
                    isEnabled = true; 
                    break;
                case "main":
                    isEnabled = true; 
                    break;
            }

            var sl = SeverityLevel.Error;
            var location = string.Empty;

            //the 3 main logs will be auto-set to on, so we just want to check if they are explicitly turned off
            //additional logs will be auto-set to off, so we have to make sure they are explicity turned on
            XmlElement enableCheck = confSection["IsEnabled"];
            if (enableCheck != null)
            {
                switch (enableCheck.InnerText.ToLower())
                {
                    case "false":
                        isEnabled = false;
                        break;
                    case "true":
                        isEnabled = true;
                        break;
                }
            }

            XmlElement verbCheck = confSection["VerbosityLevel"];
            if (verbCheck != null)
            {
                sl = TediousEnumParse(verbCheck);
            }

            XmlElement logloc = confSection["LogLocation"];
            if (logloc != null)
            {
                location = logloc.InnerText;
            }

            if (string.IsNullOrWhiteSpace(location))
                location = @"C:\Logs\" + AppDomain.CurrentDomain.FriendlyName;

            var temp = new Log4NetConfiguration
            {
                IsEnabled = isEnabled,
                LogName = confSection["LogName"].InnerText,
                LogLocation = location,
                VerbosityLevel = sl,
            };

            return temp;
        }

        public static Configuration Deserialize(string value)
        {
            var retVal = new Configuration();

            if (string.IsNullOrWhiteSpace(value))
            {
                retVal.SetDefault();
                return retVal;
            }


            //do not try to catch exceptions here, if there is a problem with the xml itself, let it fail out
            var xDoc = new XmlDocument();
            xDoc.LoadXml(value);

            if (xDoc["Configuration"]== null)
            {
                retVal.SetDefault();
                return retVal;
            }  
             
            XmlElement configBody = xDoc["Configuration"];

            var foundReq = false;
            var foundRep = false;
            var foundMain =false;

            var foundDb = false;
            var foundEmail = false;
            var foundSystem = false;

            foreach (XmlNode confSection in configBody.ChildNodes)
            {
                if (confSection == null) continue;

                switch (confSection.Name.ToLower())
                {
#region case "log4netconfiguration"
                    case "log4netconfiguration":

                        var temp = SetUpL4N(confSection);

                        if(temp==null)continue;

                        if (temp.LogName.ToLower() == "main")
                            foundMain = true;
                        else if (temp.LogName.ToLower() == "reply")
                            foundRep = true;
                        else if (temp.LogName.ToLower() == "request")
                            foundReq = true;

                        retVal.Log4NetConfigurations.Add(temp);
                        break;
#endregion
                    case "email":
                        EmailConfiguration tempEmail = SetupEmail(confSection);

                        if (tempEmail != null)
                        {
                            retVal.EmailConfiguration = tempEmail;
                            foundEmail = true;
                        }
                        break;
                    case "database":
                        DatabaseConfiguration tempDB = SetUpDatabase(confSection);

                        if (tempDB != null)
                        {
                            retVal.Database = tempDB;
                            foundDb = true;
                        }
                        break;
                    case "system":
                        var tempSys = SetUpSystem(confSection);

                        if (tempSys != null)
                        {
                            retVal.Systems = tempSys;
                            foundSystem = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            //check to make sure the basic ones have been set up
            //if not, go through defaulting them
            //this may make them useful, it may just set IsEnabled = false, depends on the specific implementation
            if (!foundMain)
            {
                retVal.SetDefaultLog4Net("Main");
            }

            if (!foundRep)
            {
                retVal.SetDefaultLog4Net("Reply");
            }

            if (!foundReq)
            {
                retVal.SetDefaultLog4Net("Request");
            }

            if (!foundDb)
            {
                retVal.SetDefaultDatabase();
            }

            if (!foundEmail)
            {
                retVal.SetDefaultEmail();
            }

            if (!foundSystem)
            {
                retVal.SetDefaultSysetm();
            } 

            return retVal;
        }
    } 
}
