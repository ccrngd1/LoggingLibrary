using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using BaseLogging.Objects;
using Newtonsoft.Json;

namespace BaseLogging.Data
{
    public class SplunkSaver : ISaveLog
    {
        private readonly BalancedUrlProvider _urlProvider;
        private readonly LoggingSettings _settings;
        private readonly List<IFinalSaveLog> _emergencyLoggers;

        private static readonly DateTime Epoc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        public SplunkSaver(LoggingSettings settings, List<IFinalSaveLog> emergencyLoggers)
        {
            _settings = settings;
            _urlProvider = new BalancedUrlProvider(_settings.Config.Splunk.URLs);
            _emergencyLoggers = emergencyLoggers;
        }

        private string PrepSplunkMessage(Log l)
        {
            var timestamp = l.TimeStamp.ToUniversalTime().Subtract(Epoc);

            var entry = new SplunkEvent
            {
                time = timestamp.TotalSeconds.ToString(CultureInfo.InvariantCulture),
                host = Dns.GetHostName(),
                source = l.ReportingService.LogService.ApplicationName,
                index = _settings.Config.Splunk.Index,
                sourcetype = l.GetType().ToString(),
                @event = l,
                fields = new SplunkEventFields
                {
                    group = "LawsonCS"
                }
            };

            string body = JsonConvert.SerializeObject(entry, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

            return body;
        }

        public void SaveLogs(Log l, string loggerInstance)
        {
            if (_settings.Config.Splunk == null) return;

            if (l.Severity < _settings.Config.Splunk.VerbosityLevel) return;

            if (!_settings.Config.Splunk.IsEnabled) return;

            var success = false;
            var url = _urlProvider.Next();

            while (!success && url != null)
            {
                try
                {
                    Post(url.Address, _settings.Config.Splunk.AuthKey, _settings.Config.Splunk.Timeout, PrepSplunkMessage(l));
                    _urlProvider.Validate(url);
                    success = true;
                }
                catch (WebException wex)
                {
                    var response = wex.Response as HttpWebResponse;

                    if (response != null)
                    {
                        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                        { 
                            if (_emergencyLoggers != null)
                            {
                                foreach (IFinalSaveLog emergencyLogger in _emergencyLoggers)
                                {
                                    emergencyLogger.EmergencySaveLog(l);
                                    emergencyLogger.EmergencySaveLog(
                                        new Log("SPLUNK - Unauthorized or Forbidden url", SeverityLevel.Fatal, l.ToString())
                                        .AddMessage(response.StatusCode.ToString(), null)
                                        .AddMessage(null, wex)
                                        .AddKVP("url", url.Address));
                                     
                                }
                            }

                            break;
                        }
                    } 

                    if (_emergencyLoggers != null)
                    {
                        foreach (IFinalSaveLog emergencyLogger in _emergencyLoggers)
                        {
                            emergencyLogger.EmergencySaveLog(l);
                            emergencyLogger.EmergencySaveLog(
                                new Log("Failed to send event to Splunk - WebException", SeverityLevel.Fatal, l.ToString())
                                .AddMessage(null,wex)
                                .AddKVP("url", url.Address));

                        }
                    }

                    _urlProvider.Invalidate(url);
                    url = _urlProvider.Next();
                }
                catch (Exception ex)
                {
                    if (_emergencyLoggers != null)
                    {
                        foreach (IFinalSaveLog emergencyLogger in _emergencyLoggers)
                        {
                            emergencyLogger.EmergencySaveLog(l);
                            emergencyLogger.EmergencySaveLog(
                                new Log("Failed to send event to Splunk - Exception", SeverityLevel.Fatal, l.ToString())
                                .AddMessage(null,ex)
                                .AddKVP("url", url.Address));

                        }
                    } 

                    _urlProvider.Invalidate(url);
                    url = _urlProvider.Next();
                }
            }

            if (url == null)
            {
                if (_emergencyLoggers != null)
                {
                    foreach (IFinalSaveLog emergencyLogger in _emergencyLoggers)
                    {
                        emergencyLogger.EmergencySaveLog(l);
                        emergencyLogger.EmergencySaveLog(
                            new Log("No valid Splunk servers found!", SeverityLevel.Fatal, l.ToString()));

                    }
                }
            }
        }

        private void Post(string url, string authenticationKey, int serverTimeoutMilliseconds, string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Headers.Add("Authorization", "Splunk " + authenticationKey);
            request.Method = "POST";
            request.KeepAlive = true;
            request.Timeout = serverTimeoutMilliseconds; 
            request.GetRequestStream().Write(bytes, 0, bytes.Length);

            using (WebResponse r = request.GetResponse())
            {
                Console.WriteLine("");
            }
        } 

        public void Flush()
        {
            return;
        }
    }

    /// <summary>
    /// provides round robin between a list of URL 
    /// if one have issues, we can mark it as bad and it will stay out of the pool for a short time
    /// </summary>
    internal class BalancedUrlProvider
    {
        private readonly IList<UrlInfo> _urls;
        private int _index;
        private readonly object _lock = new object();

        /// <summary>
        /// Ctor accepting a comma or semi-colon delimited list of URLS to balance
        /// </summary>
        /// <param name="urls"> comma or semi-colon delimited string of URLS </param>
        public BalancedUrlProvider(string urls)
        {
            if (string.IsNullOrEmpty(urls))
            {
                throw new ArgumentNullException("urls");
            }

            _urls = new List<UrlInfo>();
            foreach (string url in urls.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                _urls.Add(new UrlInfo(url.Trim()));
            }

            _index = 0;
        }

        /// <summary>
        /// Get the next URL in the pool
        /// </summary>
        /// <returns> the url info for calling an api</returns>
        public UrlInfo Next()
        {

            UrlInfo info = null;

            lock (_lock) //have to lock while we look for a new connection
            {
                var i = _index;

                while (true)
                {
                    info = _urls[i++]; //get then ext url in the list
                    if (i >= _urls.Count)
                    {
                        i = 0;
                    }

                    if (info.ValidAfter < DateTime.Now) //if the valid date is in the past, return this one
                    {
                        break;
                    }
                    if (i == _index) //if we checked all the entries, return null
                    {
                        info = null;
                        break;
                    }
                }

                _index = i;
            }

            return info; //we can return null of both end points have been down recently
        }

        /// <summary>
        /// if a connection errors out, this will mark it so it can passed over for a bit
        /// </summary>
        /// <param name="url">urlInfo that failed</param>
        public void Invalidate(UrlInfo url)
        {
            lock (_lock)
            {
                url.LastBlackoutPeriodSeconds = url.LastBlackoutPeriodSeconds <= 0 ? 15 : Math.Min(300, url.LastBlackoutPeriodSeconds * 2);
                url.ValidAfter = DateTime.Now.AddSeconds(url.LastBlackoutPeriodSeconds);
            }
        }

        /// <summary>
        /// if a connection was successful, report back so it can be placed back into the pool
        /// </summary>
        /// <param name="url">urlInfo that succeeded</param>
        public void Validate(UrlInfo url)
        {
            lock (_lock)
            {
                url.LastBlackoutPeriodSeconds = 0;
                url.ValidAfter = DateTime.MinValue;
            }
        }
    }

    internal class UrlInfo
    {
        public string Address { get; set; }

        public DateTime ValidAfter { get; set; }

        public int LastBlackoutPeriodSeconds { get; set; }

        public UrlInfo()
        {
            LastBlackoutPeriodSeconds = 0;
            ValidAfter = DateTime.MinValue;
        }

        public UrlInfo(string url)
            : this()
        {
            Address = url;
        }
    }

    internal class SplunkEvent
    {
        public object @event { get; set; }
        public string time { get; set; }
        public string host { get; set; }
        public string source { get; set; }
        public string sourcetype { get; set; }
        public SplunkEventFields fields { get; set; }
        public string index { get; set; }
    }

    internal class SplunkEventFields
    {
        public string group { get; set; }
    }
}
