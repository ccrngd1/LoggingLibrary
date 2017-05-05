using System;

namespace BaseLogging.Objects
{
    public class LogAdditionalDataKVP
    {
        public Guid LogUUID { get; private set; }
        public string Key { get; private set; }
        public string Value { get; private set; }

        private LogAdditionalDataKVP()
        { 
        }

        public LogAdditionalDataKVP(string key, string value, Log l) : this()
        {
            Key = key;
            Value = value;
            LogUUID = l.LogUUID;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Key, Value);
        } 
    }
}
