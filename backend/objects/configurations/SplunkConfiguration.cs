using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using BaseLogging.Objects;

namespace BaseLogging.Objects
{
	public class SplunkConfiguration : ConfigurationBase
	{
		[XmlElement(ElementName = "URLs")]
		public string URLs { get; set; }

		[XmlElement(ElementName = "Timeout")]
		public int Timeout { get; set; }

		[XmlElement(ElementName = "AuthKey")]
		public string AuthKey { get; set; }

		[XmlElement(ElementName = "Index")]
		public string Index { get; set; }
	}
}
