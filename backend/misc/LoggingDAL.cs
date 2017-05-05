using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq; 

using Microsoft.SqlServer.Server;

using BaseLogging.Objects;

namespace BaseLogging.Data
{
    internal class LoggingDAL
    {
        private readonly LoggingSettings _settings;

        private bool _logSvcDone;
        private bool _logSvcInstDone;

        public LoggingDAL(LoggingSettings settings)
        {
            _settings = settings;
        }

        private Log CheckLogService(LogService service)
        {
            Log retVal = null;

            if (_logSvcDone) return null;

            try
            {
                using (var conn = new SqlConnection(_settings.Config.Database.ConnectionString))
                using (var cmd = new SqlCommand("Logging.spA_CheckLogService"))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@serviceNameHash", service.ServiceHash);
                    cmd.Parameters.AddWithValue("@serviceName", service.ApplicationName);

                    var res = cmd.ExecuteNonQuery();

                    Debug.Assert(res == 0 || res == 1);
                }

                _logSvcDone = true;
            }
            catch (Exception ex)
            {
                retVal = new Log("LoggingDAL.CheckLogService", SeverityLevel.Error, service.ToString()).AddMessage(null, ex);
            }

            return retVal;
        }

        private Log CheckLogServiceInstance(LogServiceInstance serviceInstance)
        {
            Log retVal = null;

            if (_logSvcInstDone) return null;

            try
            {
                using (var conn = new SqlConnection(_settings.Config.Database.ConnectionString))
                using (var cmd = new SqlCommand("Logging.spA_CheckLogServiceInstance"))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@serviceNameInstanceHash", serviceInstance.ServiceInstanceHash);
                    cmd.Parameters.AddWithValue("@serviceHash", serviceInstance.LogService.ServiceHash);
                    cmd.Parameters.AddWithValue("@serviceInstanceUniversalPath", serviceInstance.UniversalPathName);
                    cmd.Parameters.AddWithValue("@serviceInstanceVersion", serviceInstance.Version);
                    cmd.Parameters.AddWithValue("@serviceInstanceBuildDate", null);
                    

                    cmd.ExecuteNonQuery();
                }

                _logSvcInstDone = true;
            }
            catch (Exception ex)
            {
                retVal = new Log("LoggingDAL.CheckLogServiceInstance",SeverityLevel.Error , serviceInstance.ToString()).AddMessage(null, ex);
            }

            return retVal;
        }

        public List<Log> SaveLogs(List<Log> l)
        {
            var retVal = new List<Log>(); 

            if (l == null || l.Count < 1) return retVal;

            var servLog = CheckLogService(l.Select(c => c.ReportingService.LogService).FirstOrDefault());
            var servInstLog = CheckLogServiceInstance(l.Select(c => c.ReportingService).FirstOrDefault());

            if(servLog!=null)
                retVal.Add(servLog);

            if(servInstLog!=null)
                retVal.Add(servInstLog);
            try
            {
                using (var conn = new SqlConnection(_settings.Config.Database.ConnectionString))
                using (var cmd = new SqlCommand("Logging.spA_SaveLogs"))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    var dataRecord = new List<SqlDataRecord>();
                    l.ForEach(c => dataRecord.Add(c.MapToTvp()));

                    SqlParameter sp = cmd.Parameters.AddWithValue("@logs", dataRecord);
                    sp.SqlDbType = SqlDbType.Structured;
                    sp.TypeName = "logging.AppLogs";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                if (l.Count == 1)
                {
                    retVal.Add(new Log("LoggingDAL.SaveLogs", SeverityLevel.Fatal, l[0].ToString()).AddMessage(null, ex));
                }
                else
                {
                    retVal.AddRange(SaveLogs(l.Take((int)Math.Floor((decimal)l.Count / 2)).ToList()));
                    retVal.AddRange(SaveLogs(l.Skip((int)Math.Ceiling((decimal)l.Count / 2)).ToList()));
                }
            }

            return retVal;
        }

        public List<Log> SaveStackTrace(List<LogStackLookup> l)
        {
            var retVal = new List<Log>();

            if (l == null || l.Count <= 0) return retVal;

            try
            {
                using (var conn = new SqlConnection(_settings.Config.Database.ConnectionString))
                using (var cmd = new SqlCommand("Logging.spA_SaveStackTraces"))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    var dataRecord = new List<SqlDataRecord>();
                    l.ForEach(c=> dataRecord.Add(c.MapToTvp()));
                    
                    SqlParameter sp = cmd.Parameters.AddWithValue("@stackTraces", dataRecord);
                    sp.SqlDbType = SqlDbType.Structured;
                    sp.TypeName = "logging.stackTraces";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                if (l.Count == 1)
                {
                    retVal.Add(new Log("LoggingDAL.SaveStackTrace", SeverityLevel.Fatal, l[0].ToString()).AddMessage(null, ex));
                }
                else
                {
                    retVal.AddRange(SaveStackTrace(l.Take((int)Math.Floor((decimal)l.Count / 2)).ToList()));
                    retVal.AddRange(SaveStackTrace(l.Skip((int)Math.Ceiling((decimal)l.Count / 2)).ToList()));
                }
            }

            return retVal;
        }

        public List<Log> SaveLogMessages(List<LogMessage> l)
        {
            var retVal = new List<Log>();

            if (l == null || l.Count <= 0) return retVal;

            try
            {
                using (var conn = new SqlConnection(_settings.Config.Database.ConnectionString))
                using (var cmd = new SqlCommand("Logging.spa_saveLogMsg"))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    var dataRecord = new List<SqlDataRecord>();
                    l.ForEach(c=>dataRecord.Add(c.MapToTvp()));

                    SqlParameter sp = cmd.Parameters.AddWithValue("@logMsgs", dataRecord);
                    sp.SqlDbType = SqlDbType.Structured;
                    sp.TypeName = "logging.LogMessages";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                if (l.Count ==1)
                {
                    retVal.Add(new Log("LoggingDal.SaveLogMessages", SeverityLevel.Fatal, l[0].ToString()).AddMessage(null, ex));
                }
                else
                {
                    retVal.AddRange(SaveLogMessages(l.Take((int)Math.Floor((decimal)l.Count / 2)).ToList()));
                    retVal.AddRange(SaveLogMessages(l.Skip((int)Math.Ceiling((decimal)l.Count / 2)).ToList()));
                }
            }

            return retVal;
        }
        
        public List<Log> CheckMessageLookup(List<LogMessageLookup> l)
        {
            var retVal = new List<Log>();

            if (l == null || l.Count <= 0) return retVal;

            try
            {
                using (var conn = new SqlConnection(_settings.Config.Database.ConnectionString))
                using (var cmd = new SqlCommand("Logging.spa_saveMessages"))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    var dataRecord = new List<SqlDataRecord>();
                    l.ForEach(c=>dataRecord.Add(c.MapToTvp()));

                    SqlParameter sp = cmd.Parameters.AddWithValue("@msgs", dataRecord);
                    sp.SqlDbType = SqlDbType.Structured;
                    sp.TypeName = "logging.messages";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                if (l.Count == 1)
                {
                    retVal.Add(new Log("LoggingDal.CheckMessageLookup", SeverityLevel.Fatal, l[0].ToString()).AddMessage(null, ex));
                }
                else
                {
                    retVal.AddRange(CheckMessageLookup(l.Take((int)Math.Floor((decimal)l.Count / 2)).ToList()));
                    retVal.AddRange(CheckMessageLookup(l.Skip((int)Math.Ceiling((decimal)l.Count / 2)).ToList()));
                }
            }

            return retVal;
        }

        public List<Log> SaveCallingMethodIOs(List<LogCallingMethodParameter> methodParams)
        {
            var retVal = new List<Log>();

            if (methodParams== null || methodParams.Count <= 0) return retVal;

            try
            {
                using (var conn = new SqlConnection(_settings.Config.Database.ConnectionString))
                using (var cmd = new SqlCommand("Logging.spa_SavingCallingMethodParamsIOs"))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    var dataRecord = new List<SqlDataRecord>();
                    methodParams.ForEach(c=>dataRecord.Add(c.MapToTvp()));

                    SqlParameter sp = cmd.Parameters.AddWithValue("@methodIOs", dataRecord);
                    sp.SqlDbType = SqlDbType.Structured;
                    sp.TypeName = "logging.CallingMethodIOs";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {

                if (methodParams.Count == 1)
                {
                    retVal.Add(new Log("LoggingDAL.SaveCallingMethodIOs", SeverityLevel.Fatal, methodParams[0].ToString()).AddMessage(null, e));
                }
                else
                {
                    retVal.AddRange(SaveCallingMethodIOs(methodParams.Take((int)Math.Floor((decimal)methodParams.Count / 2)).ToList()));
                    retVal.AddRange(SaveCallingMethodIOs(methodParams.Skip((int)Math.Ceiling((decimal)methodParams.Count / 2)).ToList()));
                }
            }

            return retVal;
        }

        public List<Log> SaveAdditionalDatas(List<LogAdditionalDataKVP> l)
        {
            var retVal = new List<Log>();

            if (l == null || l.Count <= 0) return retVal;

            try
            {
                using (var conn = new SqlConnection(_settings.Config.Database.ConnectionString))
                using (var cmd = new SqlCommand("Logging.spa_SaveAdditionalDatas"))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    var dataRecord = new List<SqlDataRecord>();
                    l.ForEach(c=>dataRecord.Add(c.MapToTvp()));

                    SqlParameter sp = cmd.Parameters.AddWithValue("@addlData", dataRecord);
                    sp.SqlDbType = SqlDbType.Structured;
                    sp.TypeName = "logging.AdditionalDatas";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                if (l.Count == 1)
                {
                    retVal.Add(new Log("LoggingDAL.SaveAdditionalDatas", SeverityLevel.Fatal, l[0].ToString()).AddMessage(null,e));
                }
                else
                {
                    retVal.AddRange(SaveAdditionalDatas(l.Take((int)Math.Floor((decimal)l.Count / 2)).ToList()));
                    retVal.AddRange(SaveAdditionalDatas(l.Skip((int)Math.Ceiling((decimal)l.Count / 2)).ToList()));
                }
            }

            return retVal;
        }
    }
}
