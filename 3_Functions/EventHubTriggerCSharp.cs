using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;

namespace Company.Function
{
    public static class EventHubTriggerCSharp
    {
        [FunctionName("EventHubTriggerCSharp")]
        public static void Run([EventHubTrigger("parroteventhub", Connection = "AllancasAzureTrainingEventHubs_ParrotReceiverPolicy_EVENTHUB")] string myEventHubMessage, TraceWriter log)
        {
            log.Info($"C# Event Hub trigger function processed a message: {myEventHubMessage}");
        }
    }
}
