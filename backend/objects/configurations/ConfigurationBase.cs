using System;
using System.Xml.Serialization;

namespace BaseLogging.Objects
{
    public class ConfigurationBase
    {
        private string _logName;


        [XmlElement(ElementName = "LogName")]
        public string LogName
        {
            get
            {
                return _logName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _logName = value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        [XmlElement(ElementName = "IsEnabled")]
        public bool IsEnabled { get; set; }

        [XmlElement(ElementName = "VerbosityLevel")]
        public SeverityLevel VerbosityLevel { get; set; }
    }
}
