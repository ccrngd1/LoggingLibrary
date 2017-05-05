using System.Xml.Serialization;

namespace BaseLogging.Objects
{
    public class DatabaseConfiguration : ConfigurationBase
    {
        [XmlElement(ElementName = "ConnectionString")]
        public string ConnectionString { get; set; }


        //stack cache
        [XmlElement(ElementName = "StackCacheLimit")]
        public int StackCacheLimit { get; set; }

        [XmlElement(ElementName = "StackCacheTime")]
        public int StackCacheTime { get; set; }


        //Message cache
        [XmlElement(ElementName = "MessageCacheTime")]
        public int MessageCacheTime { get; set; }

        [XmlElement(ElementName = "MessageCacheLimit")]
        public int MessageCacheLimit { get; set; }


        //Log Cache
        [XmlElement(ElementName = "LogCacheTime")]
        public int LogCacheTime { get; set; }

        [XmlElement(ElementName = "LogCacheLimit")]
        public int LogCacheLimit { get; set; }

    }
}
