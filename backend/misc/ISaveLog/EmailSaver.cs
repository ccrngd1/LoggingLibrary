using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text; 

using BaseLogging.Objects;

namespace BaseLogging.Data
{
    public class EmailSaver : ISaveLog, IFinalSaveLog
    {
        private readonly LoggingSettings _settings;
        
        private EmailSaver() { }

        public EmailSaver(LoggingSettings settings)
        {
            _settings = settings;
        }

        #region Implementation of ISaveLog

        /// <summary>
        /// ISaveLog implementation - will check IsEnabled and Verbosity level before attempting to send an email
        /// </summary>
        /// <param name="l">log object</param>
        /// <param name="loggerInstance">instance of the email logger to utilize - not utilized for now</param>
        public void SaveLogs(Log l, string loggerInstance)
        {
            if (!_settings.Config.EmailConfiguration.IsEnabled) return;
            if (l.Severity < _settings.Config.EmailConfiguration.VerbosityLevel) return;
            if (l.IsDuplicate) return;

            Email(l,
                _settings.Config.EmailConfiguration.EmailServer,
                _settings.Config.EmailConfiguration.EmailTo.Split(';').ToList(),
                _settings.Config.EmailConfiguration.EmailFrom);
        }

        public void Flush()
        {
            //no caching in email, just return out 
            return;
        }

        #endregion 

        #region Implementation of IFinalSaveLog

        /// <summary>
        /// This is one of the instances that allow us to get a log out as a last ditch effort
        /// it has no requirements on any settings, no checks on log level, nothing keeping it from getting out immediately
        /// this is intended to only be used internally in the logging library
        /// </summary>
        /// <param name="l"></param>
        public void EmergencySaveLog(Log l)
        {
            try
            {
                if (l.IsDuplicate) return;

                EmergencySaveLogProxy(l);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void EmergencySaveLogProxy(Log l)
        {
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
                    return;
            }

            Email(l,
                env + "mail.zcloud.com",
                new List<string> { "nick.lawson@lawsoncs.com" },
                "Errors@lawsoncs.com");
        }

        #endregion

        private static void Email(Log log, string emailServer, IEnumerable<string> emailAddys, string emailFrom)
        {
            var sc = new SmtpClient(emailServer);

            var msg = new MailMessage();

            foreach (var emailAddy in emailAddys)
            {
                msg.To.Add(emailAddy);
            }

            msg.From = new MailAddress(emailFrom);

            //service URL, serviceLocation, severity);
            var mailSubject = string.Format("{0} - {1}", log.ReportingService.UniversalPathName, log.Severity);

            msg.Body = ConstructMailBody(log);
            msg.Subject = mailSubject;
            msg.IsBodyHtml = true;

            sc.Send(msg);
        }

        private static string ConstructMailBody(Log log)
        {
            var mailBody = new StringBuilder();

            mailBody.Append(
                string.Format("<p>Claims service <b>{0}</b> has {2} message in <b>{1}</b></p>"
                    , log.ReportingService.LogService.ApplicationName, log.ReportingService.UniversalPathName, log.Severity));

            mailBody.Append(
                "...|<a href=\"#ErrorDetails\">Details</a> | <a href=\"#Caller\">Caller</a>  | <a href=\"#KVPS\">Additonal Data</a>  |  <a href=\"#StackTrace\">Stack</a> |...");

            mailBody.Append("<p><u>Message</u></p>");
            mailBody.Append(string.Format("<p>{0}</p>", log.LogMessages[0]));

            mailBody.Append("<p><u>Payload</u></p>");
            mailBody.Append(string.Format("<p>{0}</p>", log.InFlightPayload));

            mailBody.Append("<hr>");

            mailBody.Append("<h2><a name=\"ErrorDetails\"><u>Details</u></a></h2>");
            mailBody.Append("<table width=\"400\">");
            mailBody.Append(string.Format("<tr><td><u>Service Name:</u>  </td><td>{0}</td></tr>", log.ReportingService.LogService.ApplicationName));

            mailBody.Append(string.Format("<tr><td><u>Service Location:</u>  </td><td>{0}</td></tr>", log.ReportingService.UniversalPathName));

            mailBody.Append(string.Format("<tr><td><u>Assembly Version:</u>  </td><td>{0}</td></tr>", log.ReportingService.Version)); 

            mailBody.Append(string.Format("<tr><td><u>Timestamp:</u>  </td><td>{0}</td></tr>", log.TimeStamp));

            mailBody.Append("</table>");

            mailBody.Append("<hr>");

            mailBody.Append("<h2><a name=\"Caller\"><u>Caller</u></a></h2>");

            mailBody.Append("<table width=\"400\">");
            mailBody.Append(string.Format("<tr><td><u>Method Name:</u>  </td><td>{0}</td></tr>", log.CallingMethod));
            mailBody.Append("<tr><td><u>Parameters</u></td><td>     </td></tr>");

            foreach (var kvp in log.LogCallingMethodParameters)
            {
                mailBody.Append(string.Format("<tr><td>{0}:  </td><td>{1}</td></tr>", kvp.ParameterName, kvp.Value));
            }
            mailBody.Append("</table>");

            mailBody.Append("<hr>");

            mailBody.Append("<h2><a name=\"KVPS\"><u>Additional Key Value data</u></a></h2>");
            mailBody.Append("<table width=\"400\">");

            foreach (LogAdditionalDataKVP kvp in log.LogAdditionalData)
            {
                mailBody.Append(string.Format("<tr><td>{0}:  </td><td>{1}</td></tr>", kvp.Key, kvp.Value));
            }
            mailBody.Append("</table>");

            mailBody.Append("<hr/><p><a name=\"StackTrace\"><h2><u>Exception Details</u></h2></a></p>");

            foreach (LogMessage logMessage in log.LogMessages)
            {
                if (logMessage.LogMessageText != null)
                {
                    mailBody.Append("<p><u>Serivce Message:</u></p>");
                    mailBody.Append(logMessage.LogMessageText.MessageText);
                }

                mailBody.Append("<p><u>Stack Trace:</u></p>");

                string stackTraceMsg;

                if (logMessage.LogStackTrace == null || logMessage.LogStackTrace.StackTraceText == null)
                {
                    stackTraceMsg = "NULL";
                }
                else
                {
                    stackTraceMsg = logMessage.LogStackTrace.StackTraceText;
                }

                var splitStack = stackTraceMsg.Split(new[] { " at " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var s in splitStack)
                {
                    if (string.IsNullOrWhiteSpace(s)) continue;

                    string[] splitIn = s.Split(new[] { " in " }, StringSplitOptions.RemoveEmptyEntries);

                    mailBody.Append(string.Format("<p>AT {0} <br>", splitIn[0]));

                    if (splitIn.Length > 1)
                    {
                        mailBody.Append(string.Format("-----IN {0}", splitIn[1]));
                    }
                    mailBody.Append("</p>");
                }


                mailBody.Append(new string('-', 50));
                mailBody.Append("<br/>Inner Exception<br/>");
            }

            return mailBody.ToString();
        }
    }
}
