using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using BaseLogging.Objects;
using BaseLogging.Data;

namespace BaseLogging
{
    public static class LogWrapper
    {
        /// <summary>
        /// This method creates a log internally, builds all necessary/provided data and save it automatically
        /// </summary>
        /// <param name="severity">sev level which ties to db sev level and log4net sev level</param>
        /// <param name="message">optional: additional message to augment exception logging, this message will be first in the stack</param>
        /// <param name="ex">optional: if this is an exception log, provide exception so it can be decomposed into the correct internal structures</param>
        /// <param name="loggerInstance">which logger instance to log a message against</param>
        /// <param name="inflightPayload">string representation of the work in flight at them time of the exception</param>
        /// <param name="additionalDataKVP">optional: any additional meta data to be saved (EX cust id, claim id, etc)</param>
        /// <param name="callingMethodParameters">optional: parameter values of the method the log is being called from (EX most useful when used for debugging or exceptions to allow for quick reproduction)</param>
        /// <param name="callingMethod">The method in which log is being called</param>
        public static bool Log(SeverityLevel severity, string message, Exception ex, 
                                string loggerInstance, string inflightPayload = null, 
                                Dictionary<string, string> additionalDataKVP = null, 
                                Dictionary<string, string> callingMethodParameters = null, 
                                [CallerMemberName] string callingMethod="")
        {
            bool retVal;

            try
            {
                Log log = LogBuilder.Log(severity, inflightPayload, callingMethod).AddMessage(message, ex);

                if (additionalDataKVP != null && additionalDataKVP.Any())
                {
                    log = additionalDataKVP.Aggregate(log,
                        (current, keyValuePair) => current.AddKVP(keyValuePair.Key, keyValuePair.Value));
                }

                if (callingMethodParameters != null && callingMethodParameters.Any())
                {
                    log = callingMethodParameters.Aggregate(log,
                        (current, callingMethodParameter) =>
                            current.AddCallingMethodParameter(callingMethodParameter.Key, callingMethodParameter.Value));
                }

                log.Save(loggerInstance);

                Debug.Assert(log.Verify());

                retVal = true;
            }
            catch (Exception caughtEx)
            {
                Log l = LogBuilder.Log(SeverityLevel.Fatal,
                        "major exception at the log wrapper level", caughtEx, null, null, null, "LogWrapper.Log");
                
                EmailSaver.EmergencySaveLogProxy(l);
                SystemSaver.EmergencySaveLogProxy(l);

                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        /// This method creates a log internally, builds all necessary/provided data and save it automatically
        /// </summary>
        /// <param name="severity">sev level which ties to db sev level and log4net sev level</param>
        /// <param name="message">informational message to log - this is used when the condition isn't an exception</param>
        /// <param name="loggerInstance">which logger instance to log a message against</param>
        /// <param name="inflightPayload">string representation of the work in flight at them time of the exception</param>
        /// <param name="additionalDataKVP">optional: any additional meta data to be saved (EX cust id, claim id, etc)</param>
        /// <param name="callingMethodParameters">optional: parameter values of the method the log is being called from (EX most useful when used for debugging or exceptions to allow for quick reproduction)</param>
        /// <param name="callingMethod">The method in which log is being called</param>
        public static bool Log(SeverityLevel severity, string message, 
                                string loggerInstance, string inflightPayload=null, 
                                Dictionary<string, string> additionalDataKVP = null, 
                                Dictionary<string, string> callingMethodParameters = null, 
                                [CallerMemberName] string callingMethod="")
        {
            bool retVal;

            try
            {
                Log log = LogBuilder.Log(severity, inflightPayload, callingMethod).AddMessage(message);

                if (additionalDataKVP!=null && additionalDataKVP.Any())
                {
                    log = additionalDataKVP.Aggregate(log,
                        (current, keyValuePair) => current.AddKVP(keyValuePair.Key, keyValuePair.Value));
                }

                if (callingMethodParameters!=null && callingMethodParameters.Any())
                {
                    log = callingMethodParameters.Aggregate(log,
                        (current, callingMethodParameter) =>
                            current.AddCallingMethodParameter(callingMethodParameter.Key, callingMethodParameter.Value));
                }

                Debug.Assert(log.Verify());

                log.Save(loggerInstance);

                retVal = true;
            }
            catch (Exception caughtEx)
            {
                Log l = LogBuilder.Log(SeverityLevel.Fatal,
                        "major exception at the log wrapper level", caughtEx, null, null, null, "LogWrapper.Log");

                EmailSaver.EmergencySaveLogProxy(l);
                SystemSaver.EmergencySaveLogProxy(l);

                retVal = false;
            }

            return retVal;
        }

        public static void Flush(bool finalFlush = false)
        {
            DALCache.Instance.Flush(finalFlush);
        }
    }
}
