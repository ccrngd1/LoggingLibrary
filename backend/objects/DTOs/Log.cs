using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BaseLogging.Objects
{
    public class Log
    {
        public Guid LogUUID { get; private set; }
        public string CallingMethod { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public SeverityLevel Severity { get; private set; }
        public string InFlightPayload { get; private set; }

        public bool IsDuplicate { get; set; }
        
        public LogServiceInstance ReportingService { get; private set; }

        public readonly List<LogMessage> LogMessages;
        public readonly List<LogCallingMethodParameter> LogCallingMethodParameters;
        public readonly List<LogAdditionalDataKVP> LogAdditionalData;

        public Log(string callingMethod, SeverityLevel sev, string inFlightPayload)
        {
            Severity = sev;
            CallingMethod = callingMethod;
            LogUUID = Guid.NewGuid();
            TimeStamp = DateTime.Now;
             
            var sb = new StringBuilder(Environment.MachineName);
            sb.Append(Process.GetCurrentProcess().MainModule.FileName.Substring(2));

            ReportingService = new LogServiceInstance(new LogService(AppDomain.CurrentDomain.FriendlyName),  sb.ToString());

            LogMessages = new List<LogMessage>();
            LogCallingMethodParameters = new List<LogCallingMethodParameter>();
            LogAdditionalData = new List<LogAdditionalDataKVP>();
            InFlightPayload = inFlightPayload;
        }

        public bool Verify()
        {
            Debug.Assert(LogUUID!=null, "logUUID isn't set");
            Debug.Assert(CallingMethod!=null);
            Debug.Assert(TimeStamp!=null);
            Debug.Assert((int)Severity>0);
            Debug.Assert(LogMessages!=null);
            Debug.Assert(LogCallingMethodParameters != null);
            Debug.Assert(LogAdditionalData != null);

            return true;
        }

        public IReadOnlyList<LogMessage> LogMessagesReadOnlyList 
        {
            get { return LogMessages.AsReadOnly(); }
        }

        public IReadOnlyList<LogCallingMethodParameter> LogCallingMethodParametersReadOnlyList 
            => LogCallingMethodParameters.AsReadOnly();

        public IReadOnlyList<LogAdditionalDataKVP> LogAdditionalDataReadOnlyList 
            => LogAdditionalData.AsReadOnly();

        #region Overrides of Object

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(" ");
            sb.AppendLine(new string('*', 50));
            sb.AppendLine(string.Format("* Calling Method {0}", CallingMethod));
            sb.AppendLine(string.Format("* Severity {0}", Severity));
            sb.AppendLine(string.Format("* InFlightPayload {0}", InFlightPayload));

            var tempMsg = "NULL";

            if (LogMessages != null && LogMessages.Any())
                tempMsg = LogMessages[0].LogMessageText.MessageText;

            sb.AppendLine(string.Format("* Message {0}", tempMsg));

            sb.AppendLine(new string('*', 50));
            sb.AppendLine("* Addl Data");
            sb.AppendLine(new string('*', 50));

            if (LogAdditionalData != null && LogAdditionalData.Any())
            {
                foreach (LogAdditionalDataKVP logAdditionalDataKvp in LogAdditionalData)
                {
                    sb.AppendLine(string.Format("*Key {0}", logAdditionalDataKvp.Key));
                    sb.AppendLine(string.Format("*Value {0}", logAdditionalDataKvp.Value));
                }
            }
            else
            {
                sb.AppendLine("*NO ADDITIONAL DATA");
            }


            sb.AppendLine(new string('*', 50));
            sb.AppendLine("* Calling Parameters");
            sb.AppendLine(new string('*', 50));

            if (LogCallingMethodParameters != null && LogCallingMethodParameters.Any())
            {
                foreach (LogCallingMethodParameter logCallingMethodParameter in LogCallingMethodParameters)
                {

                    sb.AppendLine(string.Format("* Parameter Name {0}", logCallingMethodParameter.ParameterName));
                    sb.AppendLine(string.Format("* Value {0}", logCallingMethodParameter.Value));
                }
            }
            else
            {
                sb.AppendLine("* NO CALLING PARAMETER DATA");
            }


            sb.AppendLine(new string('*', 50));
            sb.AppendLine("* Exceptions");
            sb.AppendLine(new string('*', 50));

            if (LogMessages != null)
            {
                foreach (LogMessage lLogMessage in LogMessages)
                {
                    sb.AppendLine(string.Format("* Message {0}", lLogMessage.LogMessageText.MessageText));
                    sb.AppendLine(new string('-', 50));
                    sb.AppendLine("* Stack: ");

                    if (lLogMessage.LogStackTrace != null &&!string.IsNullOrWhiteSpace(lLogMessage.LogStackTrace.StackTraceText))
                    {
                        sb.AppendLine(lLogMessage.LogStackTrace.StackTraceText);
                    }
                }
            }

            sb.AppendLine(" ");
            sb.AppendLine(" ");
            sb.AppendLine(" ");

            return sb.ToString();
        }
        

        #endregion
    }
}
