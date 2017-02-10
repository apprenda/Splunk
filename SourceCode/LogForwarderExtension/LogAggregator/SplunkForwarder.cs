using System;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Apprenda.Services.Logging;
using System.Xml;
using System.IO;
using System.Threading;
using Apprenda.SaaSGrid.Extensions.DTO;

namespace LogAggregator
{
    class SplunkJSON
    {
        public string AccessToken { get; set; }
        public string SplunkEndpoint { get; set; }
    }

    internal class SplunkForwarder
    {
        private readonly string splunkEndpoint;
        private readonly string splunkAccessToken;        

        internal SplunkForwarder()
        {
            // Make sure you bypass some of the SSL validation since the Splunk certificate is a self-signed certificate
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new
                RemoteCertificateValidationCallback(
                       delegate { return true; }
                );

            var splunkEnvironmentVariableJSON = ReadConfigSettings();
            SplunkJSON splunk = JsonConvert.DeserializeObject<SplunkJSON>(splunkEnvironmentVariableJSON);
            this.splunkAccessToken = string.Format("Splunk {0}", splunk.AccessToken);
            this.splunkEndpoint = string.Format("{0}?channel={1}", splunk.SplunkEndpoint, LogService.logToken);
        }

        internal void ForwardLogs()
        {
            do
            {
                Action pushLogsToSplunkAction = () =>
                {
                    LogMessageDTO logMessageObj;
                    while (LogService.taskQueue.TryDequeue(out logMessageObj))
                    {
                        try
                        {
                            // put the object into JSON format and send it to Splunk
                            string logMessageJson = JsonConvert.SerializeObject(logMessageObj);                           
                            byte[] byteArray = null;
                            HttpWebRequest request = HttpWebRequest.Create(this.splunkEndpoint) as HttpWebRequest;
                            request.Method = "POST";
                            request.KeepAlive = false;
                            request.Headers.Add("Authorization", this.splunkAccessToken);
                            request.ContentType = "application/json";
                            byteArray = Encoding.UTF8.GetBytes(logMessageJson);
                            request.ContentLength = byteArray.Length;
                            request.GetRequestStream().Write(byteArray, 0, byteArray.Length);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                            // keep some success and failure metrics
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                ++LogService.successfulForwards;
                            }
                            else
                            {
                                ++LogService.failedForwards;
                            }

                            response.Close();                                                      
                        }
                        catch (Exception e)
                        {
                            // can't really log this exception to avoid cyclical logging and deadlocks
                            ++LogService.numExceptions;                           
                        }
                    }
                };

                // Start 5 parallel tasks to consume the logs from the queue
                // the problem here is that the logs might not be pushed in order to Splunk 
                // However, that's ok since there might be multiple instances of this service and logs could be coming
                // in to all instances at the same time and we might push logs to Splunk out of order
                Parallel.Invoke(pushLogsToSplunkAction, pushLogsToSplunkAction, pushLogsToSplunkAction, pushLogsToSplunkAction, pushLogsToSplunkAction);

                // sleep before getting into another iteration of this loop to look for logs
                Thread.Sleep(1000);
            } while (true);                                       
        }

        private string ReadConfigSettings()
        {
            string configDir = AppDomain.CurrentDomain.BaseDirectory;
            var logger = LogManager.Instance().GetLogger(typeof(LogService));
            logger.Log(configDir, LogLevel.Fatal);

            string[] configFiles = Directory.GetFiles(configDir, "Splunk.config", SearchOption.TopDirectoryOnly);
            if (configFiles.Length != 1)
            {
                throw new Exception("Failed to find the Splunk.config file at " + configDir);
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configFiles[0]);
            XmlNode appsettingsNode = xmlDoc.SelectSingleNode("//appSettings");
            if (null == appsettingsNode)
            {
                throw new Exception("Failed to find the appSettings node in Splunk.config file at " + configDir);
            }

            XmlElement connectionElement = (XmlElement)xmlDoc.SelectSingleNode("//appSettings/add[@key = 'SplunkConnectionDetails']");
            if (null == connectionElement)
            {
                throw new Exception("Failed to find the Splunk elements in node in Splunk.config file at " + configDir);
            }
            
            return connectionElement.Attributes["value"].Value;            
        }
    }
}