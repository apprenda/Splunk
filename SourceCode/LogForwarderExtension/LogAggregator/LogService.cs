using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Apprenda.SaaSGrid.Extensions;
using Apprenda.SaaSGrid.Extensions.DTO;
using Apprenda.Services.Logging;
using LogAggregatorTests;
using System.Threading;

namespace LogAggregator
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class LogService : LogAggregatorExtensionServiceBase, ILogTestService
    {
        internal static ConcurrentQueue<LogMessageDTO> taskQueue = new ConcurrentQueue<LogMessageDTO>();
        internal static long successfulForwards = 0;
        internal static long failedForwards = 0;
        internal static long numExceptions = 0;
        internal static readonly Guid logToken = Guid.NewGuid();

        public LogService()
        {               
            // start a 2nd thread with the forwarder class for Splunk
            // evaluate if starting many threads will forward the data to splunk faster
            // we don't want the data to stay in memory for too long to avoid data loss
            SplunkForwarder splunkFwd = new SplunkForwarder();
            Thread splunkThread = new Thread(new ThreadStart(splunkFwd.ForwardLogs));
            splunkThread.Start();
        }
        
        // OnLogsPersisted is called from the platform when log forwarding is enabled
        // We want to as quickly as possible push the logs to the thread safe
        // concurrent queue and let the other thread consume the logs, put them in
        // JSON format, and push them to an external logging service like Splunk
        public override void OnLogsPersisted(IEnumerable<LogMessageDTO> logs)
        {           
            foreach (LogMessageDTO logObj in logs)
            {
                taskQueue.Enqueue(logObj);
            }
        }

        // Test Method to log a few log messages to validate the functionality
        public Guid WarmService(int numOfLogs)
        {
            var logger = LogManager.Instance().GetLogger(typeof(LogService));
            for (var i = 0; i < numOfLogs; i++)
            {
                logger.Log(logToken, LogLevel.Fatal);
            }
                       
            return logToken;
        }

        // Test Method to provide the current count of the queue service and other statistics
        public string GetStatistics()
        {
            lock (taskQueue)
            {
                return string.Format("QueueCount={0}, Exceptions={1}, FailedUploads={2}, SuccessfulUpload={3}",
                    taskQueue.Count(), numExceptions, failedForwards, successfulForwards);
            }
        }       
    }
}
