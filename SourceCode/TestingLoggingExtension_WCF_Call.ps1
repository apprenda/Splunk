# Details about the Apprenda application that implemented the Apprenda Extension
$applicationName = "logforwarder"
$applicationversionalias = "v1"

# Details about the Apprenda Cloud Platform environment hosting the Apprenda Extension application
$requestURI = "http://apps.apprenda.harp/api/services/soap11/http/r/$applicationName($applicationversionalias)/LogService/ILogTestService" 

# Load the assemblies necessary to get the WCF types needed below
[Reflection.Assembly]::LoadFrom("LogForwarderExtension\packages\SaaSGrid.API.6.6.2\lib\net45\SaaSGrid.API.dll")
[Reflection.Assembly]::LoadFrom("LogForwarderExtension\LogAggregator\bin\Debug\LogAggregator.dll")
[Reflection.Assembly]::LoadWithPartialName(“System.ServiceModel”)

# Make API calls to the Testing Interface of the Logging extension
$httpBinding = New-Object System.ServiceModel.BasicHttpBinding
$endpointAddress = New-Object System.ServiceModel.EndpointAddress $requestURI
$contractDescription = [System.ServiceModel.Description.ContractDescription]::GetContract([LogAggregatorTests.ILogTestService])
$serviceEndpoint = New-Object System.ServiceModel.Description.ServiceEndpoint($contractDescription, $httpBinding, $endpointAddress)
$channelFactory = New-Object "System.ServiceModel.ChannelFactory``1[LogAggregatorTests.ILogTestService]" $serviceEndpoint
$webProxy = $channelFactory.CreateChannel()

# Ask the testing interface for the logging statistics
$webProxy.GetStatistics()

# Ask the testing interface to log 10 sample messages for testing
$webProxy.WarmService(10)

# Ask the testing interface for the logging statistics (again)
$webProxy.GetStatistics()

[Console]::In.ReadLine()
