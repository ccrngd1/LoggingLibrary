using System; 
using System.Security.Cryptography;
using System.Text; 

namespace BaseLogging.Objects
{
    public class LogServiceInstance
    {
        private string _serviceInstanceHash;
        public string ServiceInstanceHash
        {
            get
            {
                //if the backing field isn't set yet
                //and the messageText has already been set
                //hash the messageText and save the result here so it can be returned quickly on next call
                if (string.IsNullOrWhiteSpace(_serviceInstanceHash))
                {
                    if (!string.IsNullOrWhiteSpace(UniversalPathName))
                    {
                        using (SHA512 shaM = new SHA512Managed())
                        {
                            byte[] hashArray = shaM.ComputeHash(Encoding.UTF8.GetBytes(UniversalPathName+Version));
                            _serviceInstanceHash = Encoding.Default.GetString(hashArray);
                        }
                    }
                    else
                    {
                        throw new NullReferenceException("LogMessageLookup.MessageText not set");
                    }
                }
                return _serviceInstanceHash;
            }
            private set { throw new InvalidOperationException(); }
        }

        public string UniversalPathName { get; private set; }

        private string _version;
        public string Version
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_version))
                    
                    _version = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion;
                return _version;
            }
        } 

        public LogService LogService { get; private set; }

        public LogServiceInstance(LogService service, string servicePath)
        {
            LogService = service;
            UniversalPathName = servicePath;
        }

        #region Overrides of Object

        public override string ToString()
        {
            return string.Format("{0}[{1}]", UniversalPathName, ServiceInstanceHash);
        }

        #endregion
    }
}
