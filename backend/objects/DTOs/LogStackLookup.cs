using System;
using System.Security.Cryptography;
using System.Text; 

namespace BaseLogging.Objects
{
    public class LogStackLookup
    {
        private string _stackHash;
        public string StackHash
        {
            get
            {
                //if the backing field isn't set yet
                //and the messageText has already been set
                //hash the messageText and save the result here so it can be returned quickly on next call
                if (!string.IsNullOrWhiteSpace(_stackHash)) return _stackHash;

                if (!string.IsNullOrWhiteSpace(StackTraceText))
                {
                    using (SHA512 shaM = new SHA512Managed())
                    {
                        byte[] hashArray = shaM.ComputeHash(Encoding.UTF8.GetBytes(StackTraceText));
                        _stackHash = System.Text.Encoding.Default.GetString(hashArray);
                    }
                }
                else
                {
                    throw new NullReferenceException("LogMessageLookup.MessageText not set");
                }

                return _stackHash;
            }
            private set { throw new InvalidOperationException(); }
        }

        public string StackTraceText { get; private set; }

        public LogStackLookup(string traceText)
        {
            if (traceText == null)
                traceText = "NULL";

            StackTraceText = traceText;
        }
    }
}
