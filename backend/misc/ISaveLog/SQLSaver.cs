using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using BaseLogging.Objects;

namespace BaseLogging.Data
{
    internal class SQLSaver : ISaveLog
    {
        private readonly ConcurrentStack<Log> _cachedLogs;
        private readonly ConcurrentStack<LogStackLookup> _cachedStackLookups;
        private readonly ConcurrentStack<LogMessageLookup> _cachedMessageLookups;

        private readonly List<IFinalSaveLog> _emergencyLoggers;
        private readonly LoggingDAL _dal;
        private readonly LoggingSettings _settings;

        private DateTime _lastMsgDump;
        private DateTime _lastStackDump;
        private DateTime _lastLogDump; 

        public SQLSaver(LoggingDAL dal, List<IFinalSaveLog> emergencyLoggers )
        {
            _emergencyLoggers = emergencyLoggers;
            _cachedMessageLookups = new ConcurrentStack<LogMessageLookup>();
            _cachedStackLookups = new ConcurrentStack<LogStackLookup>();
            _cachedLogs = new ConcurrentStack<Log>();

            _dal = dal;
            _settings = LoggingSettings.Instance;

            _lastLogDump = new DateTime();
            _lastStackDump = new DateTime();
            _lastMsgDump = new DateTime();
        }

        public void SaveLogs(Log l, string loggerInstance)
        {
            if (_settings.Config.Database == null) return;

            if (l.Severity < _settings.Config.Database.VerbosityLevel) return;

            if (!_settings.Config.Database.IsEnabled) return;

            _cachedLogs.Push(l);

            foreach (LogMessage msg in l.LogMessages)
            {
                if (msg.LogMessageText != null && !_cachedMessageLookups.Contains(msg.LogMessageText))
                    _cachedMessageLookups.Push(msg.LogMessageText);

                if (msg.LogStackTrace != null && !_cachedStackLookups.Contains(msg.LogStackTrace))
                    _cachedStackLookups.Push(msg.LogStackTrace);
            }

            Flush(false);
        }

        public void Flush()
        {
            Flush(true);
        }
        
        private volatile object _flushLock = new object();
        private volatile bool _isFlushing ;

        private void Flush(bool force)
        {
            if(_isFlushing) return;

            lock (_flushLock)
            {
                _isFlushing = true;
                //write logs
                if (_cachedLogs.Count > _settings.Config.Database.LogCacheLimit ||
                    DateTime.Now > _lastLogDump.AddSeconds(_settings.Config.Database.LogCacheTime) ||
                    force)
                {
                    _lastLogDump = DateTime.Now;

                    //we have to save off the child items off from the log file before we save the logs and dispense of them
                    List<Log> exceptionsForMsgs =_dal.SaveLogMessages(_cachedLogs.SelectMany(c => c.LogMessages).ToList());
                    List<Log> exceptionsForAddlDatas = _dal.SaveAdditionalDatas(_cachedLogs.SelectMany(c => c.LogAdditionalData).ToList());
                    List<Log> exceptionsForMethods = _dal.SaveCallingMethodIOs(_cachedLogs.SelectMany(c => c.LogCallingMethodParameters).ToList());

                    if (_emergencyLoggers != null)
                    {
                        _emergencyLoggers.ForEach(c=>exceptionsForMsgs.ForEach(c.EmergencySaveLog));
                        _emergencyLoggers.ForEach(c=>exceptionsForAddlDatas.ForEach(c.EmergencySaveLog));
                        _emergencyLoggers.ForEach(c=> exceptionsForMethods.ForEach(c.EmergencySaveLog));
                    }
                    
                    List<Log> pulledLogs = new List<Log>();
                    Log temp;

                    while (_cachedLogs.TryPop(out temp))
                    {
                        if (temp == null) break;

                        pulledLogs.Add(temp);
                    }
                        
                    //save the actual log off and dispense of it
                    var failedSaves = _dal.SaveLogs(pulledLogs);

                    if (_emergencyLoggers != null)
                    {
                        _emergencyLoggers.ForEach(c=>failedSaves.ForEach(c.EmergencySaveLog));
                    }
                }

                //write stack
                if (_cachedStackLookups.Count > _settings.Config.Database.StackCacheLimit ||
                    DateTime.Now > _lastStackDump.AddSeconds(_settings.Config.Database.StackCacheTime) ||
                    force)
                {
                    _lastStackDump = DateTime.Now;

                    var pulledStackLookups = new List<LogStackLookup>();
                    LogStackLookup temp;

                    while (_cachedStackLookups.TryPop(out temp))
                    {
                        pulledStackLookups.Add(temp);
                    }

                    var excpetionsForStkLookup = _dal.SaveStackTrace(pulledStackLookups);

                    if (_emergencyLoggers != null)
                    {
                        _emergencyLoggers.ForEach(c => excpetionsForStkLookup.ForEach(c.EmergencySaveLog));
                    }
                }

                //write msgs
                if (_cachedMessageLookups.Count > _settings.Config.Database.MessageCacheLimit ||
                    DateTime.Now > _lastMsgDump.AddSeconds(_settings.Config.Database.MessageCacheTime) ||
                    force)
                {
                    _lastMsgDump = DateTime.Now;

                    var pulledMsgLookups = new List<LogMessageLookup>();

                    LogMessageLookup temp;

                    while (_cachedMessageLookups.TryPop(out temp))
                    {
                        pulledMsgLookups.Add(temp);
                    }

                    var exceptionsForMsgLookup = _dal.CheckMessageLookup(pulledMsgLookups);

                    if (_emergencyLoggers != null)
                    {
                        _emergencyLoggers.ForEach(c=>exceptionsForMsgLookup.ForEach(c.EmergencySaveLog));
                    }
                }
                _isFlushing = false;
            }
        }
    }
}
