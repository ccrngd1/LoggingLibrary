using System.Collections.Generic;

namespace BaseLogging.Objects
{
    public interface ISaveLog
    {
        void SaveLogs(Log l, string loggerInstance);
        void Flush();
    }

    public interface IFinalSaveLog
    {
        /// <summary>
        /// This is the last attempt to save a log.  Everything else has gone wrong and we have no way of knowing what or dealing with it.
        /// Any implementation of this should be completely decoupled from any dependency in the log library, such as the settings
        /// On inception, this will cover only console.Write and Email
        /// </summary>
        /// <param name="l">the log consisting only of the unhandlable, intra-library exception</param>
        void EmergencySaveLog(Log l); 
    }
}
