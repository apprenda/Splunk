# Splunk
This is the repository that includes all the details to integrate Apprenda logging with the Splunk operational intelligence platform. With Apprenda version 6.6, we enabled the capability to forward all logs that the Apprenda Platform collects to a WCF service which can then forward them to any logging subsystem. All logs includes the logs from guest applications as well as the logs from the core platform services.

This repository will walk you through the steps to set up this integration as well as provide the sample code to get you started.

## Code Repository
- Splunk Add-On, an Apprenda Add-On that allows an operator to define the details of a provisioned Splunk account so that developers can use it inside a guest application
- LogForwarderExtension WCF Service, An Apprenda WCF service that is built as an extension. It receives all the logs from Apprenda and forwards them to Splunk

## Integration Steps, Setting up Splunk
- First, go ahead and create a Splunk account, provision an instance and create an `HTTP Event Collector` under "Data Inputs". You can also create an index and tie that index to the HTTP Event Collector. In my case, I created an index called "apprendalogs"
- Splunk will provide you with a Token Value, for example a45912b9-617a-457f-8ac9-4e6ab9f5dc4a, for accessing the HTTP Event Collector. You will need that value along with the full FQDN of your Splunk instance to be able to send logs to it. In our case, I will use the raw endpoint of the HTTP Event Collector at https://input-prd-p-uniqueID.cloud.splunk.com:8088/services/collector/raw
- You can learn more about the HTTP Event Collector at http://dev.splunk.com/view/event-collector/SP-CAAAE6M

## Integration Steps, Setting up the Apprenda Add-On in the Apprenda Operator Portal
- Use the provided Apprenda.Splunk.AddOn.zip to upload the Add-On to the Apprenda SOC (aka Operator Portal). You can alternatively build or enhance the provided Visual Studio solution file to create an Add-On that meets your needs.
- Once the Add-On is uploaded in Apprenda, edit it and visit the "Configuration" tab
- In the `Splunk HTTP Event Collector URL` field, enter the Splunk-instance-specific URL from above, https://input-prd-p-uniqueID.cloud.splunk.com:8088/services/collector/raw
- In the `Splunk HTTP Event Collector Access Token Value`, enter the Splunk Access Token for the HTTP Event Collector, a45912b9-617a-457f-8ac9-4e6ab9f5dc4a
- Save the Add-On
- You can learn more about Add-Ons at http://docs.apprenda.com/6-5/addons

## Integration Steps, Setting up the Apprenda Add-On in the Apprenda Developer Portal
- Now visit the Apprenda Developer Portal and click on "Add-Ons"
- 
