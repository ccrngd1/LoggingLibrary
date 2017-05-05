using System.Xml.Serialization;

namespace BaseLogging.Objects
{
    public class Log4NetConfiguration : ConfigurationBase
    {
        [XmlElement(ElementName = "LogLocation")]
        public string LogLocation; 
    }
}
