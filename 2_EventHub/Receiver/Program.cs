using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Receiver
{
    class Program
    {
        private static IConfiguration Configuration;

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            ConfigureProgram();
            await ProcessEvents();

            Console.WriteLine("Quitting the program");
        }

        private static async Task ProcessEvents()
        {
            Console.WriteLine("Registering EventProcessor...");

            var eventProcessorHost = new EventProcessorHost(
                Configuration["ParrotEventHubEntityPath"],
                PartitionReceiver.DefaultConsumerGroupName,
                Configuration["ParrotEventHubReceiverConnectionString"],
                Configuration["ConnectionString"],
                Configuration["ParrotEventHubReceiverEventProcessorHostContainer"]);

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();

            Console.WriteLine("Receiving. Press ENTER to stop worker.");
            Console.ReadLine();

            // Disposes of the Event Processor Host
            await eventProcessorHost.UnregisterEventProcessorAsync();
        }

        private static void ConfigureProgram()
        {
            // Configuration object for loading in azure keyvault uri
            var kvbuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var kvconfig = kvbuilder.Build();
            var azureKVUri = kvconfig["azure_keyvault_uri"];

            // Build configuration object that can access AzureKeyVault
            var builder = new ConfigurationBuilder()
                .AddAzureKeyVault(azureKVUri);
            Configuration = builder.Build();
        }
    }
}
