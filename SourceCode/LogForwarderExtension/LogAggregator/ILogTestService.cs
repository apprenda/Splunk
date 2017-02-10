using System;
using System.ServiceModel;

namespace LogAggregatorTests
{
    [ServiceContract(Namespace = ("LogAggregatorTests"), Name = "ILogTestService")]
    public interface ILogTestService
    {
        [OperationContract]
        Guid WarmService(int numOfLogs);

        [OperationContract]
        string GetStatistics();
    }
}
