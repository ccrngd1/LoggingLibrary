using System.Xml.Serialization;

namespace BaseLogging.Objects
{
    public class EmailConfiguration : ConfigurationBase
    {
        [XmlElement(ElementName = "Name")]
        public string EmailName { get; set; }

        [XmlElement(ElementName = "EmailTo")]
        public string EmailTo { get; set; }

        [XmlElement(ElementName = "EmailFrom")]
        public string EmailFrom { get; set; }

        [XmlElement(ElementName = "EmailServer")]
        public string EmailServer { get; set; }
    }
}