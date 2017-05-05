using System; 
using System.Security.Cryptography;
using System.Text; 

namespace BaseLogging.Objects
{
    public class LogService
    {
        private string _serviceHash;
        public string ServiceHash
        {
            get
            {
                //if the backing field isn't set yet
                //and the messageText has already been set
                //hash the messageText and save the result here so it can be returned quickly on next call
                if (string.IsNullOrWhiteSpace(_serviceHash))
                {
                    if (!string.IsNullOrWhiteSpace(ApplicationName))
                    {
                        using (SHA512 shaM = new SHA512Managed())
                        {
                            byte[] hashArray = shaM.ComputeHash(Encoding.UTF8.GetBytes(ApplicationName));
                            _serviceHash = Encoding.Default.GetString(hashArray);
                        }
                    }
                    else
                    {
                        throw new NullReferenceException("CurrentLogService.ApplicationName not set");
                    }
                }
                return _serviceHash;
            }
            private set { throw new InvalidOperationException(); }
        }

        public string ApplicationName { get; private set; }

        public LogService(string serviceName)
        {
            ApplicationName = serviceName;
        }

        #region Overrides of Object

        public override string ToString()
        {
            return string.Format("{0}[{1}]", ApplicationName, ServiceHash);
        }

        #endregion
    }
}
