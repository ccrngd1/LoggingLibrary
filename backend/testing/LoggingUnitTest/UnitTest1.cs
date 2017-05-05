using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using log4net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using BaseLogging;
using BaseLogging.Data;
using BaseLogging.Objects;
using BaseLogging.UnitTest;


namespace Claims.Common.Logging.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CreateLogSimpleSignature()
        { 
            Log log = LogBuilder.Log(SeverityLevel.Debug,""); 

            Assert.IsNotNull(log);
            Assert.IsTrue(log.Verify());
        }

        [TestMethod]
        public void CreateLogFullSignatureWithMessage()
        { 
            Log log = LogBuilder.Log(SeverityLevel.Debug, "", new Dictionary<string,string>(), new Dictionary<string, string>(),"");

            Assert.IsNotNull(log);
            Assert.IsTrue(log.Verify());
        }

        [TestMethod]
        public void CreateLogFullSignatureWWithException()
        { 
            Log log = LogBuilder.Log(SeverityLevel.Debug, "", new Exception(),"", new Dictionary<string, string>(), new Dictionary<string, string>());

            Assert.IsNotNull(log);
            Assert.IsTrue(log.Verify());
        }

        [TestMethod]
        public void CreateLogMessageExtension()
        { 
            Log log = LogBuilder.Log(SeverityLevel.Debug,"");
            
            Assert.IsNotNull(log);
            Assert.IsTrue(log.Verify());

            log.AddMessage("test");

            Assert.IsNotNull(log.LogMessagesReadOnlyList.Count>0);
            Assert.IsTrue(log.LogMessagesReadOnlyList[0].LogMessageText.MessageText == "test");
            Assert.IsNotNull(log.LogMessagesReadOnlyList[0].LogMessageText.LogMessageHash);
        }

        [TestMethod]
        public void CreateLogMsgWithExceptionWithNoStackTrace()
        {
            Log log = LogBuilder.Log(SeverityLevel.Debug, "No payload");

            log.AddMessage("no msg", new Exception());

            Assert.IsTrue(log.LogMessages.Any());

            Assert.IsTrue(log.LogMessages[0].LogMessageText!=null);
            Assert.IsTrue(log.LogMessages[0].LogMessageText.MessageText!=null);
            Assert.IsTrue(log.LogMessages[0].LogMessageText.LogMessageHash!= null);

            Assert.IsTrue(log.LogMessages[1].LogStackTrace!= null);
            Assert.IsTrue(log.LogMessages[1].LogStackTrace.StackTraceText != null);
            Assert.IsTrue(log.LogMessages[1].LogStackTrace.StackHash != null);

            log = LogBuilder.Log(SeverityLevel.Debug, "no payload");

            log.AddMessage(null, new Exception());

            Assert.IsTrue(log.LogMessages[0].LogMessageText!=null);

            log = LogBuilder.Log(SeverityLevel.Debug, "no payload");

            log.AddMessage(null, new Exception("outter", new Exception("inner")));

            Assert.IsTrue(log.LogMessages[0].LogMessageText != null);
            Assert.IsTrue(log.LogMessages[0].LogMessageText.MessageText == "outter");

            Assert.IsTrue(log.LogMessages[1].LogMessageText != null);
            Assert.IsTrue(log.LogMessages[1].LogMessageText.MessageText == "inner");
        }

        [TestMethod]
        public void DuplicationUnitTest()
        {
            DALCache cacheInst = DALCache.Instance;
            LoggingSettings settingsInst = LoggingSettings.Instance;

            settingsInst.DuplicateCachePurgeSeconds = 1;

            Log log = LogBuilder.Log(SeverityLevel.Debug, "No payload", "CreateLogMsgWithExceptionWithNoStackTrace");
            log.AddMessage("no msg", new Exception());

            //first check - should not be duplicate
            var logCount = cacheInst.DetermineDuplicateState(log);
            Assert.IsFalse(log.IsDuplicate);
            Assert.IsTrue(logCount == 1);

            //second check - immediate - should be duplicate
            logCount = cacheInst.DetermineDuplicateState(log);
            Assert.IsTrue(log.IsDuplicate);
            Assert.IsTrue(logCount>1);

            //wait for the over the purge limit, then try again
            Task.Delay(2000).Wait();
            logCount = cacheInst.DetermineDuplicateState(log);
            Assert.IsFalse(log.IsDuplicate);
            Assert.IsTrue(logCount == 1);

            //hit again immediately - should be marked as a duplicate
            logCount = cacheInst.DetermineDuplicateState(log);
            Assert.IsTrue(log.IsDuplicate);
            Assert.IsTrue(logCount > 1); 
        }

        [TestMethod]
        public void DeserializeXMLConfig()
        {
            try
            {
                Assert.IsTrue(LoggingSettings.Instance.Config.EmailConfiguration != null);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }


        #region manual/dev testing
#if DEBUG
        [TestMethod] 
        public void LogWrapperSingle()
        {
            Assert.IsTrue(LogWrapper.Log(SeverityLevel.Debug, "", "", "", new Dictionary<string, string>(), new Dictionary<string, string>()));
        }
         
        [TestMethod] 
        public void SaveToLogFileOnly_WithAppConfigPresent ()
        {
            var configWRapperTest = new LogConfigWrapperTests();
            configWRapperTest.SerializeToXMLTest();

            LoggingSettings.Instance.Config.Database.IsEnabled = false;
            LoggingSettings.Instance.Config.EmailConfiguration.IsEnabled = false;

            LogWrapper.Log(SeverityLevel.Debug, "testing",
                            new Exception("Is this real or just a fantasy"),
                            LoggerInstanceConstants.MainLoggerInstance,
                            @"1021391480þ923703585þ1þ605þ605þ3þ0þZþþþþþþ05/04/2016 15:52:59.6942þ12þVIDAKOVIC, LYNNþþMEDICAL SERVICES RICþþþþ0þR-ZþüPAYER NAME MATCHING REQUIRED.üýüBILLING NAME MATCHING REQUIRED.üþ05/04/2016þ15:52:59.6942þ1021391480.1þþ114X67712481þ246.00þREHAB INST OF CHICAGOþPþþ3þ1244þþ1205879095þ1659306587þ1114933660þ0þ605þþþþþþþþþþþþ20160504þZ1ýZ1þþPAYER NAME MATCHING REQUIREDýBILLING NAME MATCHING REQUIRED",
                            new Dictionary<string, string> { { "Additional Data 1", "SomeImportantValue" }, { "TheAnswer", "42" }, { "HAP Status", "I am online, Dave" } },
                            new Dictionary<string, string> { { "InputParamter1", "True" }, { "NextParam", "String" }, { "NullableParm", null } },
                            "UnitTest.SaveToALogFileOnly");

            log4net.Config.XmlConfigurator.Configure();

            LogManager.GetLogger("").Debug("something");


            LogWrapper.Log(SeverityLevel.Debug, "testing",
                            new Exception("Is this real or just a fantasy"),
                            LoggerInstanceConstants.MainLoggerInstance,
                            @"1021391480þ923703585þ1þ605þ605þ3þ0þZþþþþþþ05/04/2016 15:52:59.6942þ12þVIDAKOVIC, LYNNþþMEDICAL SERVICES RICþþþþ0þR-ZþüPAYER NAME MATCHING REQUIRED.üýüBILLING NAME MATCHING REQUIRED.üþ05/04/2016þ15:52:59.6942þ1021391480.1þþ114X67712481þ246.00þREHAB INST OF CHICAGOþPþþ3þ1244þþ1205879095þ1659306587þ1114933660þ0þ605þþþþþþþþþþþþ20160504þZ1ýZ1þþPAYER NAME MATCHING REQUIREDýBILLING NAME MATCHING REQUIRED",
                            new Dictionary<string, string> { { "Additional Data 1", "SomeImportantValue" }, { "TheAnswer", "42" }, { "HAP Status", "I am online, Dave" } },
                            new Dictionary<string, string> { { "InputParamter1", "True" }, { "NextParam", "String" }, { "NullableParm", null } },
                            "UnitTest.SaveToALogFileOnly");

        }
         
        [TestMethod] 
        public void SaveToALogFileOnly()
        {
            var configWRapperTest = new LogConfigWrapperTests();
            configWRapperTest.SerializeToXMLTest();

            LoggingSettings.Instance.Config.Database.IsEnabled = false;
            LoggingSettings.Instance.Config.EmailConfiguration.IsEnabled = false;

            LogWrapper.Log(SeverityLevel.Debug, "testing",
                            new Exception("Is this real or just a fantasy", new NullReferenceException("Bound to happen")),
                            LoggerInstanceConstants.MainLoggerInstance,
                            @"1021391480þ923703585þ1þ605þ605þ3þ0þZþþþþþþ05/04/2016 15:52:59.6942þ12þVIDAKOVIC, LYNNþþMEDICAL SERVICES RICþþþþ0þR-ZþüPAYER NAME MATCHING REQUIRED.üýüBILLING NAME MATCHING REQUIRED.üþ05/04/2016þ15:52:59.6942þ1021391480.1þþ114X67712481þ246.00þREHAB INST OF CHICAGOþPþþ3þ1244þþ1205879095þ1659306587þ1114933660þ0þ605þþþþþþþþþþþþ20160504þZ1ýZ1þþPAYER NAME MATCHING REQUIREDýBILLING NAME MATCHING REQUIRED",
                            new Dictionary<string, string> { { "Additional Data 1", "SomeImportantValue" }, { "TheAnswer", "42" }, { "HAP Status", "I am online, Dave" } },
                            new Dictionary<string, string> { { "InputParamter1", "True" }, { "NextParam", "String" }, { "NullableParm", null } },
                            "UnitTest.SaveToALogFileOnly");

            //Task.Delay(1000*60).Wait();



            LogWrapper.Log(SeverityLevel.Debug, "testing2",
                            new Exception("Is this real or just a fantasy"),
                            LoggerInstanceConstants.MainLoggerInstance,
                            @"1021391480þ923703585þ1þ605þ605þ3þ0þZþþþþþþ05/04/2016 15:52:59.6942þ12þVIDAKOVIC, LYNNþþMEDICAL SERVICES RICþþþþ0þR-ZþüPAYER NAME MATCHING REQUIRED.üýüBILLING NAME MATCHING REQUIRED.üþ05/04/2016þ15:52:59.6942þ1021391480.1þþ114X67712481þ246.00þREHAB INST OF CHICAGOþPþþ3þ1244þþ1205879095þ1659306587þ1114933660þ0þ605þþþþþþþþþþþþ20160504þZ1ýZ1þþPAYER NAME MATCHING REQUIREDýBILLING NAME MATCHING REQUIRED",
                            new Dictionary<string, string> { { "Additional Data 1", "SomeImportantValue" }, { "TheAnswer", "42" }, { "HAP Status", "I am online, Dave" } },
                            new Dictionary<string, string> { { "InputParamter1", "True" }, { "NextParam", "String" }, { "NullableParm", null } },
                            "UnitTest.SaveToALogFileOnly");

        }
         
        [TestMethod] 
        public void SaveToEmailOnly()
        {
            var configWrapTest = new LogConfigWrapperTests();
            configWrapTest.SerializeToXMLTest();

            bool all = LoggingSettings.Instance.Config.Log4NetConfigurations.All(c => c.IsEnabled = false);

            Assert.IsFalse(all);

            LoggingSettings.Instance.Config.Database.IsEnabled = false;
            LoggingSettings.Instance.Config.EmailConfiguration.IsEnabled = true;

            try
            {
                throw new Exception("Is this real or just a fantasy");
            }
            catch (Exception ex)
            {
                LogWrapper.Log(SeverityLevel.Debug, "testing", ex,
                    LoggerInstanceConstants.MainLoggerInstance,
                    @"1021391480þ923703585þ1þ605þ605þ3þ0þZþþþþþþ05/04/2016 15:52:59.6942þ12þVIDAKOVIC, LYNNþþMEDICAL SERVICES RICþþþþ0þR-ZþüPAYER NAME MATCHING REQUIRED.üýüBILLING NAME MATCHING REQUIRED.üþ05/04/2016þ15:52:59.6942þ1021391480.1þþ114X67712481þ246.00þREHAB INST OF CHICAGOþPþþ3þ1244þþ1205879095þ1659306587þ1114933660þ0þ605þþþþþþþþþþþþ20160504þZ1ýZ1þþPAYER NAME MATCHING REQUIREDýBILLING NAME MATCHING REQUIRED",
                    new Dictionary<string, string>
                    {
                        {"Additional Data 1", "SomeImportantValue"},
                        {"TheAnswer", "42"},
                        {"HAP Status", "I am online, Dave"}
                    },
                    new Dictionary<string, string>
                    {
                        {"InputParamter1", "True"},
                        {"NextParam", "String"},
                        {"NullableParm", null}
                    },
                    "UnitTest.SaveToEmailOnly");
            }
        }
         
        [TestMethod] 
        public void SaveToDBOnly()
        {
            var configWrapTest = new LogConfigWrapperTests();
            configWrapTest.SerializeToXMLTest();

            bool allEnabled = LoggingSettings.Instance.Config.Log4NetConfigurations.All(c => c.IsEnabled = false);

            Assert.IsTrue(allEnabled);

            LoggingSettings.Instance.Config.EmailConfiguration.IsEnabled = false;

            try
            {
                throw new Exception("Is this real or just a fantasy");
            }
            catch (Exception ex)
            {
                LogWrapper.Log(SeverityLevel.Debug, "testing", ex,
                    LoggerInstanceConstants.MainLoggerInstance,
                    @"1021391480þ923703585þ1þ605þ605þ3þ0þZþþþþþþ05/04/2016 15:52:59.6942þ12þVIDAKOVIC, LYNNþþMEDICAL SERVICES RICþþþþ0þR-ZþüPAYER NAME MATCHING REQUIRED.üýüBILLING NAME MATCHING REQUIRED.üþ05/04/2016þ15:52:59.6942þ1021391480.1þþ114X67712481þ246.00þREHAB INST OF CHICAGOþPþþ3þ1244þþ1205879095þ1659306587þ1114933660þ0þ605þþþþþþþþþþþþ20160504þZ1ýZ1þþPAYER NAME MATCHING REQUIREDýBILLING NAME MATCHING REQUIRED",
                    new Dictionary<string, string>
                    {
                        {"Additional Data 1", "SomeImportantValue"},
                        {"TheAnswer", "42"},
                        {"HAP Status", "I am online, Dave"},
                        {"oversize", new string('p',5000) },
                        {new string('t',5000), "oversize name" }
                    },
                    new Dictionary<string, string>
                    {
                        {"InputParamter1", "True"},
                        {"NextParam", "String"},
                        {"NullableParm", null},
                        {"oversize", new string('p',5000) },
                        {new string('t',5000), "oversize name" }
                    },
                    "UnitTest.SaveToDBOnly");

                try
                {
                    throw new Exception("it's inception");
                }
                catch (Exception ex2)
                {
                    LogWrapper.Log(SeverityLevel.Debug, "testing2", ex2,
                        LoggerInstanceConstants.MainLoggerInstance,
                        @"BAH",
                        new Dictionary<string, string>
                        {
                        {"Additional Data 1", "SomeImportantValue"},
                        {"TheAnswer", "42"},
                        {"HAP Status", "I am online, Dave"},
                        {"oversize", new string('p',5000) },
                        {new string('t',5000), "oversize name" }
                        },
                        new Dictionary<string, string>
                        {
                        {"InputParamter1", "True"},
                        {"NextParam", "String"},
                        {"NullableParm", null},
                        {"oversize", new string('p',5000) },
                        {new string('t',5000), "oversize name" }
                        },
                        "UnitTest.SaveToDBOnly");
                }

                //LogWrapper.Flush();
            }
            Task.Delay(1000 * 60).Wait();
            LogWrapper.Flush(true);
            Task.Delay(1000 * 60).Wait();
        } 

        [TestMethod] 
        public void TestFlushPoller()
        {
            try
            {
                var temp = DALCache.Instance;

                Assert.IsNotNull(temp);

                Task.Delay(1000*60*2).Wait();

                DALCache.Instance.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
#endif
#endregion
    }
}

