using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace BaseLogging.Objects
{
    public static class LogWrapperFluentExtensions
    {
        public static void Serialize<T>(this T value, string filePath)
        {
            if (value == null)
            {
                throw new NullReferenceException();
            }

            var xmlSerializer = new XmlSerializer(typeof(T));

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (XmlReader reader = XmlReader.Create(fs))
                {
                    xmlSerializer.Serialize(fs, value);
                }
            }
        }

        public static T Deserialize<T>(this string value)
        {
            T retVal;

            var deserializer = new XmlSerializer(typeof(T));

            using (XmlReader reader = XmlReader.Create(value))
            {
                object obj = deserializer.Deserialize(reader);
                retVal = (T)obj;
            }

            return retVal;
        }

        public static Log AddStackTrace(this Log log, string stackTrace)
        {
            log.LogMessages.Last().LogStackTrace = new LogStackLookup (stackTrace);

            return log;
        }

        public static Log AddMessage(this Log log, string message)
        {
            log.LogMessages.Add(new LogMessage(log, message));

            return log;
        }

        public static Log AddMessage(this Log log, string message, Exception ex)
        {
            if(!string.IsNullOrWhiteSpace(message))
                log.LogMessages.Add(new LogMessage(log, message));


            Exception innerE = null;

            if (ex != null)
            {
                log.LogMessages.Add(new LogMessage(log, null, ex));
                innerE = ex.InnerException;
            }

            //get the inner exceptions
            while (innerE != null)
            {
                log.LogMessages.Add(new LogMessage(log, null, innerE));
                innerE = innerE.InnerException;
            }

            return log;
        }

        public static Log AddKVP(this Log log, string key, string value)
        {
            log.LogAdditionalData.Add(new LogAdditionalDataKVP(key, value,log));

            return log;
        }

        public static Log AddCallingMethodParameter(this Log log, string parameterName, string parameterValue, bool isInput=true)
        {
            log.LogCallingMethodParameters.Add(new LogCallingMethodParameter(parameterValue, parameterName, log, isInput));

            return log;
        }  
    }
}
