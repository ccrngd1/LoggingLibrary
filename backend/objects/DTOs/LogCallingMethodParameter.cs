namespace BaseLogging.Objects
{
    public class LogCallingMethodParameter
    {
        public bool IsInput { get; internal set; }
        public Log OwnerLog { get; internal set; }
        public string Value { get; internal set; }
        public string ParameterName { get; internal set; }

        public LogCallingMethodParameter(string value, string parameterName, Log log, bool isInput = true)
        {
            if (string.IsNullOrWhiteSpace(value))
                value = "NULL";

            IsInput = isInput;
            Value = value;
            ParameterName = parameterName;
            OwnerLog = log;
        }
    }
}
