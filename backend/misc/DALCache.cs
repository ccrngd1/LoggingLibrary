using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading; 

using BaseLogging.Objects; 

namespace BaseLogging.Data
{
    public class CacheEntry
    {
        public CacheEntry()
        {
            InitialDateTime = DateTime.Now;
            Count = 1;
        }

        public DateTime InitialDateTime { get; }
        public int Count { get; set; }
    }
    public class DALCache : IDisposable
    {
        private static volatile DALCache _instance;
        private static readonly object SyncRoot = new object();

        private readonly LoggingSettings _logSettings = LoggingSettings.Instance;

        private static ConcurrentDictionary<string, CacheEntry> _logDuplicationCache;
         
        private readonly Timer _flushTimer; 
        
        #region ISavers
        private readonly List<ISaveLog> _logSavers;
        private readonly List<IFinalSaveLog> _emergencySavers;

        private readonly Log4NetSaveLog _log4Saver; 
        private readonly SQLSaver _logSqlSaver;
        private readonly SystemSaver _systemSaver;
        private readonly EmailSaver _emailSaver;
        #endregion  

        private DALCache()
        {
            LoggingSettings settings = LoggingSettings.Instance;
             
            if (_log4Saver ==null)
                _log4Saver = new Log4NetSaveLog(settings); 

            if(_systemSaver == null)
                _systemSaver = new SystemSaver(settings);

            if(_emailSaver == null)
                _emailSaver = new EmailSaver(settings);


            if (_emergencySavers == null)
            {
                _emergencySavers = new List<IFinalSaveLog>
                {
                    _systemSaver,
                    _emailSaver
                };
            }

            if (_logSqlSaver == null)
                _logSqlSaver = new SQLSaver(new LoggingDAL(settings), _emergencySavers);

            if (_logSavers == null)
            {
                _logSavers = new List<ISaveLog>
                {
                    _logSqlSaver,
                    _log4Saver,
                    _systemSaver,
                    _emailSaver
                };
            }

            _flushTimer = new Timer(FlushPoller, null, 0, Timeout.Infinite);
        }

        private void FlushPoller(object state)
        {
            //disable our timer while we work
            _flushTimer.Change(Timeout.Infinite, Timeout.Infinite);

            foreach (ISaveLog logSaver in _logSavers)
            {
                try
                {
                    logSaver.Flush();
                }
                catch (Exception ex)
                {
                    _emergencySavers.ForEach(
                        c =>
                            c.EmergencySaveLog(
                                new Log("DALCache.FlushPoller", SeverityLevel.Fatal, null).AddMessage(null, ex)));
                }
            }

            _flushTimer.Change(1000*60*5, Timeout.Infinite);
        }

        public static DALCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new DALCache();
                            _logDuplicationCache = new ConcurrentDictionary<string, CacheEntry>();
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// determine whether a log has been sent recently
        /// will return the number of times this log message has been encountered since the purge
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public int DetermineDuplicateState(Log log)
        {
            //make sure it is unset, then determine if we should turn it back on
            log.IsDuplicate = false;

            CacheEntry returnedEntry = null;

            //determine if this log is a duplicate that is already being tracked
            try
            {
                if (log.LogMessages.Any() && log.LogMessages[0].LogMessageText != null)
                {
                    _logDuplicationCache.TryGetValue(log.LogMessages[0].LogMessageText.LogMessageHash, out returnedEntry);

                    if (returnedEntry != null && DateTime.Now >
                        returnedEntry.InitialDateTime.AddSeconds(_logSettings.DuplicateCachePurgeSeconds))
                    {
                        _logDuplicationCache.TryRemove(log.LogMessages[0].LogMessageText.LogMessageHash,
                            out returnedEntry);
                    } 
                    
                    //no matter if we removed above or just need to update
                    //single call will do both for us
                    //if we removed, will re-add immediately
                    //if existing, will update count
                    returnedEntry = _logDuplicationCache.AddOrUpdate(log.LogMessages[0].LogMessageText.LogMessageHash,
                        new CacheEntry(), 
                        (k, v) =>
                        {
                            v.Count++;
                            return v;
                        });}
            }
            catch (Exception ex)
            {
                _emergencySavers.ForEach(
                    c =>
                        c.EmergencySaveLog(
                            new Log("DALCache.DetermineDuplicateState", SeverityLevel.Fatal, null).AddMessage(null, ex)));
            }

            //if this is null somehow, assume it wasnt in the cache
            if (returnedEntry == null) return -1;

            //since we should have that instantiated
            //if there is more than 1, its a dup
            //return the count out just for a bit of visibility and unit testability 
            log.IsDuplicate = returnedEntry.Count > 1;
            return returnedEntry.Count;
        }

        public void SaveLog(Log log, string loggerInstance)
        {
            //do our duplication checking
            DetermineDuplicateState(log);

            foreach (ISaveLog saver in _logSavers)
            {
                try
                {
                    saver.SaveLogs(log, loggerInstance);
                }
                catch (Exception ex)
                {
                    _emergencySavers.ForEach(
                        c =>
                            c.EmergencySaveLog(
                                new Log("DALCache.SaveLog", SeverityLevel.Fatal, null).AddMessage(null, ex)));
                }
            }
        }

        public void Flush(bool finalFlush=false)
        {
            if (finalFlush)
                _flushTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            foreach (ISaveLog logSaver in _logSavers)
            {
                try
                {
                    logSaver.Flush();
                }
                catch(Exception ex)
                {
                    _emergencySavers.ForEach(
                        c =>
                            c.EmergencySaveLog(
                                new Log("DALCache.Flush", SeverityLevel.Fatal, null).AddMessage(null, ex)));
                }
            }
        }

        #region dispose
        // Flag: Has Dispose already been called? 
        private bool _disposed;

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        private void Dispose(bool disposing)
        {
            try
            {
                if (_disposed) return;

                if (!disposing) return;

                Flush(true);

                _flushTimer.Dispose();

                _instance = null;  

                // Free any unmanaged objects here. 
                //
            }
            catch (Exception ex)
            {
                _emergencySavers.ForEach(
                    c =>
                        c.EmergencySaveLog(new Log("DALCache.Dispose", SeverityLevel.Fatal, null).AddMessage(null, ex)));
            }

            finally
            {
                //finish
                _disposed = true;
            }
        }

        ~DALCache()
        {
            Dispose(false);
        }
        #endregion
    } 
}
