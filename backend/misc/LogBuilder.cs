using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using BaseLogging.Objects;
using BaseLogging.Data;

namespace BaseLogging
{
    public static class LogBuilder
    {
        /// <summary>
        /// This method creates a log and returns the new instance requring the caller to build additional data and manually call the save functionality when ready
        /// </summary>
        /// <param name="severity">sev level which ties to db sev level and log4net sev level</param>
        /// <param name="inflightPayload">string representation of the work in flight at them time of the exception</param>
        /// <param name="callingMethod">The method in which log is being called</param>
        /// <returns>Log instance for manually building up via extensions and saving via extension method</returns>
        public static Log Log(SeverityLevel severity, string inflightPayload=null, [CallerMemberName] string callingMethod ="")
        {
            Log log = null;
            try
            {
                log = new Log(callingMethod, severity, inflightPayload);
            }
            catch (Exception caughtEx)
            {
                Log l = Log(SeverityLevel.Fatal,
                        "major exception at the log wrapper level", caughtEx, null, null, null, "LogBuilder.Log");
                 
                    EmailSaver.EmergencySaveLogProxy(l); 
                    SystemSaver.EmergencySaveLogProxy(l);
            }


            return log;
        }

        /// <summary>
        /// This method creates a log and returns the new instance requring the caller to build additional data and manually call the save functionality when ready
        /// </summary>
        /// <param name="severity">sev level which ties to db sev level and log4net sev level</param>
        /// <param name="message">optional: additional message to go along with exception, this will be shown as the top most message in the stack</param>
        /// <param name="ex">optional: if this is an exception log, provide exception so it can be decomposed into the correct internal structures</param>
        /// <param name="inflightPayload">string representation of the work in flight at them time of the exception</param>
        /// <param name="additionalDataKVP">optional: any additional meta data to be saved (EX cust id, claim id, etc)</param>
        /// <param name="callingMethodParameters">optional: parameter values of the method the log is being called from (EX most useful when used for debugging or exceptions to allow for quick reproduction)</param>
        /// <param name="callingMethod">The method in which log is being called</param>
        public static Log Log(SeverityLevel severity, string message, 
                                Exception ex, string inflightPayload, 
                                Dictionary<string, string> additionalDataKVP, 
                                Dictionary<string, string> callingMethodParameters, 
                                [CallerMemberName] string callingMethod="")
        {
            Log log = null;
            try
            {
                log = Log(severity, inflightPayload, callingMethod).AddMessage(message, ex);

                if (additionalDataKVP != null && additionalDataKVP.Any())
                {
                    log = additionalDataKVP.Aggregate(log,
                        (current, keyValuePair) => current.AddKVP(keyValuePair.Key, keyValuePair.Value));
                }

                if (callingMethodParameters != null && callingMethodParameters.Any())
                {
                    log = callingMethodParameters.Aggregate(log,
                        (current, callingMethodParameter) =>
                            current.AddCallingMethodParameter(callingMethodParameter.Key,
                                callingMethodParameter.Value));
                }

                Debug.Assert(log.Verify());
            }
            catch (Exception caughtEx)
            {
                Log l = Log(SeverityLevel.Fatal,
                        "major exception at the log wrapper level", caughtEx, null, null, null, "LogBuilder.Log");

                EmailSaver.EmergencySaveLogProxy(l);
                SystemSaver.EmergencySaveLogProxy(l);
            }

            return log;
        }

        /// <summary>
        /// This method creates a log and returns the new instance requring the caller to build additional data and manually call the save functionality when ready
        /// </summary>
        /// <param name="severity">sev level which ties to db sev level and log4net sev level</param>
        /// <param name="message">informational message to log - this is used when the condition isn't an exception</param>
        /// <param name="additionalDataKVP">optional: any additional meta data to be saved (EX cust id, claim id, etc)</param>
        /// <param name="callingMethodParameters">optional: parameter values of the method the log is being called from (EX most useful when used for debugging or exceptions to allow for quick reproduction)</param>
        /// <param name="inflightPayload">string representation of the work in flight at them time of the exception</param>
        /// <param name="callingMethod">The method in which log is being called</param>
        public static Log Log(SeverityLevel severity, string message, 
                                Dictionary<string, string> additionalDataKVP, 
                                Dictionary<string, string> callingMethodParameters, 
                                string inflightPayload, [CallerMemberName] string callingMethod="")
        {
            Log log = null;

            try
            {
                log = Log(severity, inflightPayload, callingMethod).AddMessage(message);

                if (additionalDataKVP != null && additionalDataKVP.Any())
                {
                    log = additionalDataKVP.Aggregate(log,
                        (current, keyValuePair) => current.AddKVP(keyValuePair.Key, keyValuePair.Value));
                }

                if (callingMethodParameters != null && callingMethodParameters.Any())
                {
                    log = callingMethodParameters.Aggregate(log,
                        (current, callingMethodParameter) =>
                            current.AddCallingMethodParameter(callingMethodParameter.Key,
                                callingMethodParameter.Value));
                }

                Debug.Assert(log.Verify());

            }
            catch (Exception caughtEx)
            {
                Log l = Log(SeverityLevel.Fatal,
                        "major exception at the log wrapper level", caughtEx, null, null, null, "LogWrapper.Log");

                EmailSaver.EmergencySaveLogProxy(l);
                SystemSaver.EmergencySaveLogProxy(l);
            }
            return log;
        }

        public static void Flush()
        {
            DALCache.Instance.Flush();
        }
    }
}
