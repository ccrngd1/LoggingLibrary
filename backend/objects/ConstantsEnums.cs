namespace BaseLogging.Objects
{
    public static class LoggerInstanceConstants
    {
        public const string MainLoggerInstance = "Main";
        public const string RequestLoggerInstance = "Request";
        public const string ReplyLoggerInstance = "Reply";
    }

    public enum SeverityLevel
    {
        Debug=1,
        Warn,
        Info,
        Error,
        Fatal,
    }
}
