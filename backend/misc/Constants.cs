using System.Data;
using Microsoft.SqlServer.Server;

namespace BaseLogging
{
    internal static class Constants
    {
        internal static readonly SqlMetaData[] LogMetadata = {
            new SqlMetaData("UUID", SqlDbType.UniqueIdentifier),
            new SqlMetaData("ServiceInstanceHash", SqlDbType.Char,64),
            new SqlMetaData("SeverityLevelId", SqlDbType.Int),
            new SqlMetaData("CallingMethodName", SqlDbType.VarChar, 255),
            new SqlMetaData("LogTimeStamp", SqlDbType.DateTime),
            new SqlMetaData("InFlightPayload", SqlDbType.VarChar,-1 ),
        };

        internal static readonly SqlMetaData[] StackLookupMetadata =
        {
            new SqlMetaData("stackTraceHash", SqlDbType.Char,64),
            new SqlMetaData("stackText", SqlDbType.VarChar ,-1),
        };

        internal static readonly SqlMetaData[] MessageMetadata =
        {
            new SqlMetaData("logUUID", SqlDbType.UniqueIdentifier),
            new SqlMetaData("msgLookupHash", SqlDbType.Char, 64),
            new SqlMetaData("depth", SqlDbType.Int),
            new SqlMetaData("stackTraceHash", SqlDbType.Char, 64),
        };

        internal static readonly SqlMetaData[] MessageLookupMetaData =
        {
            new SqlMetaData("messageHash", SqlDbType.Char, 64),
            new SqlMetaData("messageText", SqlDbType.VarChar ,-1),
        };

        internal static readonly SqlMetaData[] CallingMethodIOData =
        {
            new SqlMetaData("logUUID", SqlDbType.UniqueIdentifier),
            new SqlMetaData("IsInput", SqlDbType.Bit),
            new SqlMetaData("parameterName", SqlDbType.VarChar, 255),
            new SqlMetaData("Value", SqlDbType.VarChar ,-1),
        };

        internal static readonly SqlMetaData[] AddlDataMetaData =
        {
            new SqlMetaData("logUUID", SqlDbType.UniqueIdentifier),
            new SqlMetaData("KeyName", SqlDbType.VarChar, 255),
            new SqlMetaData("Value", SqlDbType.VarChar ,-1),
        };
    }
}
