using Microsoft.Azure.EventHubs;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sender
{
    class Program
    {
        private static EventHubClient eventHubClient;
        private static IConfiguration Configuration;

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            ConfigureProgram();

            while (true)
            {
                Console.WriteLine("Enter a message (or \"quit\" to quit)");
                var text = Console.ReadLine();
                if (text == "quit"){
                    break;
                }

                await SendMessageToEventHub(text);
            }

            Console.WriteLine("Quitting the program");
            await eventHubClient.CloseAsync();
        }

        // Sends message to the event hub.
        private static async Task SendMessageToEventHub(string data)
        {
            try
            {
                var message = $"Message {data}";
                Console.WriteLine($"Sending message: {message}");
                await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
            }

            await Task.Delay(10);

            Console.WriteLine($"Message sent.");
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

            // Configure eventHubClient
            eventHubClient = EventHubClient.CreateFromConnectionString(Configuration["ParrotEventHubSenderConnectionString"]);
        }
    }
}
