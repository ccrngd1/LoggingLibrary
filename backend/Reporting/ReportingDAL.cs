using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;  

namespace Claims.Common.Logging.Reporting
{
    public struct LogHolder
    {
        public string sevLevelId;
        public string callingMethodName;
        public string InFlightPayload;
        public string timeStamp;
        public string appPath;
        public string appVersion;
        public string buildDate;
        public string appName;
        public string msgText;
        public string stackText;
    }

    public class ReportingDAL
    {
        private readonly string _connString;

        public ReportingDAL(string conn)
        {
            _connString = conn;
        }

        public List<LogHolder> GetReportsForTimeSpan(DateTime start, DateTime end, int lowestIncludedLogLevel=3)
        {
            var logs = new List<LogHolder>();

            string sqlFailOver = @"SELECT 
       appLog.SeverityLevelId ,
       appLog.CallingMethodName ,
       appLog.InFlightPayload ,
       appLog.LogTimeStamp ,
       Inst.UniversalAppPath ,
       Inst.AppVersion , 
       Serv.AppName,
       msgLookup.MessageText,
       stackTrace.StackText
       
FROM ClaimsAuditWarehouse.Logging.AppLog appLog
left JOIN ClaimsAuditWarehouse.Logging.LogServiceInstances Inst ON inst.LogServiceInstanceHash= appLog.ServiceInstanceHash
left JOIN ClaimsAuditWarehouse.Logging.LogServices Serv ON serv.ServiceHash= inst.LogServiceHash
left JOIN ClaimsAuditWarehouse.Logging.LogMessages msgs ON msgs.LogUUID = appLog.UUID
LEFT JOIN ClaimsAuditWarehouse.Logging.StackTraceLookup stackTrace ON stackTrace.Hash = msgs.StackTraceHash
LEFT JOIN ClaimsAuditWarehouse.Logging.MessageLookup msgLookup ON msgLookup.Hash = msgs.LogMessageLookupHash
WHERE LogTimeStamp > @startDT AND LogTimeStamp < @endDT
AND appLog.SeverityLevelId >= @logLvl";

            using (var conn = new SqlConnection(_connString))
            //using (var cmd = new SqlCommand("logging.GetLogsForTimeSpan"))
            using (var cmd = new SqlCommand(sqlFailOver))
            {
                conn.Open();
                cmd.Connection = conn;

                cmd.Parameters.AddWithValue("@startDT", start);
                cmd.Parameters.AddWithValue("@endDT", end);
                cmd.Parameters.AddWithValue("@logLvl", lowestIncludedLogLevel);

                cmd.CommandType = CommandType.Text;


                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var temp = new LogHolder
                        {
                            sevLevelId = reader["SeverityLevelId"].ToString(),
                            callingMethodName = reader["CallingMethodName"].ToString(),
                            InFlightPayload = reader["InFlightPayload"].ToString(),
                            timeStamp = reader["LogTimeStamp"].ToString(),
                            appPath = reader["UniversalAppPath"].ToString(),
                            appVersion = reader["AppVersion"].ToString(),
                            //buildDate = reader["BuildDate"].ToString(),
                            appName = reader["AppName"].ToString(),
                            msgText = reader["MessageText"].ToString(),
                            stackText = reader["StackText"].ToString(),
                        };
                        logs.Add(temp);
                    }
                }
            }
            return logs;
        }
    }
}