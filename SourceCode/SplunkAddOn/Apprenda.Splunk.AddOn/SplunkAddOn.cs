using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprenda.SaaSGrid.Addons;

namespace Apprenda
{
    public class SplunkAddOn : AddonBase
    {
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            /*The Deprovision method allows you to specify the steps taken when a developer deprovisions his/her Add-On. 
             * You should use this step to clean up any provisioned artifacts. The connectiondata object inside the request should
             * have all the information needed to clean up the provisioned resource. 
             * At the end, you can return whether the operation was successful or not as shown in the sample below*/
            return new OperationResult { EndUserMessage = "The Add-On was deprovisioned successfully", IsSuccess = true };
        }

        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            /*The Provision method provisions the instance for the service you are setting up. Within this method, you can access the information
             requested from the developer (if any) by iterating through the request.DeveloperParameters object.
             * At the end of the provisioning process, simply return the connection information for the service that was provisioned.*/
            //Retrieving developer parameters     
            //var parameter = request.DeveloperParameters.First(param => param.Key == "RequiredParameter");

            // Retrieving manifest properties
            var splunkURL = request.Manifest.Properties.First(param => param.Key == "SplunkServerURL");
            var splunkToken = request.Manifest.Properties.First(param => param.Key == "SplunkHTTPToken");
            
            var result = new ProvisionAddOnResult(string.Format("{{'SplunkEndpoint': '{0}', 'AccessToken': '{1}'}}", splunkURL.Value, splunkToken.Value), true, string.Format("The Add-On was provisioned successfully."));
            return result;
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            /*The test method allows you to test whether the Add-On was developed and configured properly and that any dependent systems are
             operating normally. During this method, you will want to go through a similar workflow to Provision to ensure proper functioning
             * of the Add-On.*/
            return new OperationResult { EndUserMessage = "The Add-On was tested successfully", IsSuccess = true };
        }
    }
}
