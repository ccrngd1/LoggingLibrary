using Microsoft.SqlServer.Server;

using BaseLogging.Objects;

namespace BaseLogging.Data
{
    public static class LogWrapperFluentExtensionsDAL
    {
        public static void Save(this Log log, string loggerInstance = null)
        {
            if (loggerInstance == null)
            {
                loggerInstance = LoggerInstanceConstants.MainLoggerInstance;
            }

            DALCache.Instance.SaveLog(log, loggerInstance);
        }

        internal static SqlDataRecord MapToTvp(this Log message)
        {
            if (message == null) return null;

            var record = new SqlDataRecord(Constants.LogMetadata);

            record.SetSqlGuid(0, message.LogUUID);

            var sih = "NULL";

            if (message.ReportingService != null &&
                !string.IsNullOrWhiteSpace(message.ReportingService.ServiceInstanceHash))
                sih = message.ReportingService.ServiceInstanceHash;

            record.SetString(1, sih);

            record.SetInt32(2, (int)message.Severity);

            if (!string.IsNullOrWhiteSpace(message.CallingMethod))
                record.SetString(3, message.CallingMethod.Substring(0, message.CallingMethod.Length > 100 ? 100:message.CallingMethod.Length));
            else
                record.SetDBNull(3);

            record.SetDateTime(4, message.TimeStamp);
            
            if (message.InFlightPayload == null)
                record.SetDBNull(5);
            else
                record.SetString(5, message.InFlightPayload);

            return record;
        }

        internal static SqlDataRecord MapToTvp(this LogStackLookup stackLookup)
        {
            var record = new SqlDataRecord(Constants.StackLookupMetadata);

            record.SetString(0, stackLookup.StackHash);
            record.SetString(1, stackLookup.StackTraceText);

            return record;
        }

        internal static SqlDataRecord MapToTvp(this LogMessage message)
        {
            var record = new SqlDataRecord(Constants.MessageMetadata);

            record.SetGuid(0, message.OwnerLog.LogUUID);
            record.SetString(1, message.LogMessageText.LogMessageHash);
            record.SetInt32(2, message.Depth);

            if(message.LogStackTrace==null)
                record.SetDBNull(3);
            else 
                record.SetString(3, message.LogStackTrace.StackHash);

            return record;
        }

        internal static SqlDataRecord MapToTvp(this LogMessageLookup message)
        {
            var record = new SqlDataRecord(Constants.MessageLookupMetaData);

            record.SetString(0, message.LogMessageHash);
            record.SetString(1, message.MessageText);

            return record;
        }

        internal static SqlDataRecord MapToTvp(this LogCallingMethodParameter message)
        {
            var recrod = new SqlDataRecord(Constants.CallingMethodIOData);

            recrod.SetGuid(0, message.OwnerLog.LogUUID);
            recrod.SetBoolean(1, message.IsInput);
            recrod.SetString(2, message.ParameterName);
            recrod.SetString(3, message.Value);

            return recrod;
        }

        internal static SqlDataRecord MapToTvp(this LogAdditionalDataKVP addlData)
        {
            var record = new SqlDataRecord(Constants.AddlDataMetaData);

            record.SetGuid(0, addlData.LogUUID);
            record.SetString(1, addlData.Key);
            record.SetString(2, addlData.Value);

            return record;

        }
    }
}
