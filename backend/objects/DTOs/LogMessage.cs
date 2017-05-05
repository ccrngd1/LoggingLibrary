using System;
using System.Security.Cryptography;
using System.Text; 

namespace BaseLogging.Objects
{
    public class LogMessage
    {
        public Log OwnerLog { get; internal set; }
        public int Depth { get; internal set; }  

        public LogMessageLookup LogMessageText { get; internal set; }

        public LogStackLookup LogStackTrace { get; internal set; }

        public override string ToString()
        {
            var sb = new StringBuilder(LogMessageText.MessageText);
            sb.Append(Environment.NewLine);

            if(LogStackTrace!=null)
                sb.Append(LogStackTrace.StackTraceText);

            return sb.ToString();
        }

        public LogMessage(Log log, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "NULL";

            LogMessageText = new LogMessageLookup(message);

            Depth = 0;

            if(log.LogMessages!=null)
                Depth = log.LogMessages.Count;

            OwnerLog = log;
        }

        public LogMessage(Log log, string message, Exception ex)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                if (ex != null)
                    message = ex.Message;
                else
                    message = "NULL";
            }
            
            LogMessageText = new LogMessageLookup(message);

            if (ex != null)
            {
                LogStackTrace = new LogStackLookup(ex.StackTrace);
            }

            Depth = 0;

            if (log.LogMessages != null)
                Depth = log.LogMessages.Count;

            OwnerLog = log;
        }
    }

    public class LogMessageLookup
    {
        private string _logMessageHash;

        public string LogMessageHash
        {
            get
            {
                //if the backing field isn't set yet
                //and the messageText has already been set
                //hash the messageText and save the result here so it can be returned quickly on next call
                if (!string.IsNullOrWhiteSpace(_logMessageHash)) return _logMessageHash;

                if (!string.IsNullOrWhiteSpace(MessageText))
                {
                    using (SHA512 shaM = new SHA512Managed())
                    {
                        byte[] hashArray = shaM.ComputeHash(Encoding.UTF8.GetBytes(MessageText));
                        _logMessageHash = System.Text.Encoding.Default.GetString(hashArray);
                    }
                }
                else
                {
                    throw new NullReferenceException("LogMessageLookup.MessageText not set");
                }

                return _logMessageHash;
            }
            private set { throw new InvalidOperationException(); }
        }

        public string MessageText { get; private set; }

        public LogMessageLookup(string messageText)
        {
            MessageText = messageText;
        }
    }
}
