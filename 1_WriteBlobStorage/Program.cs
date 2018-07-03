using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WriteBlobStorage
{
    class Program
    {
        public static IConfiguration Configuration { get; set; }

        static void Main(string[] args)
        {
            // Configuration object for loading in azure keyvault uri
            var kvbuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var kvconfig = kvbuilder.Build();
            var azureKVUri = kvconfig["azure_keyvault_uri"];

            // Load configuration objects
            var builder = new ConfigurationBuilder()
                .AddAzureKeyVault(azureKVUri);
            Configuration = builder.Build();

            var connectionStr = Configuration["ConnectionString"];
            var blobName = Configuration["BlobName"];

            var cloudBlobContainer = WriteText(connectionStr, blobName).GetAwaiter().GetResult();
            if (cloudBlobContainer == null) // Never wrote anything
                return;

            var readStr = ReadBlobText(cloudBlobContainer, blobName).GetAwaiter().GetResult();
            if (!String.IsNullOrEmpty(readStr))
                Console.WriteLine("The string we downloaded was: {0}", readStr);
            else
                Console.WriteLine("ERROR: We couldn't read a string");

            DeleteBlob(cloudBlobContainer, blobName).GetAwaiter().GetResult();
        }

        private static async Task<CloudBlobContainer> WriteText(string connectionStr, string blobName)
        {
            // Console.WriteLine("Enter a string you want to upload to Azure!");
            // var uploadStr = Console.ReadLine();
            var uploadStr = "Hey Azure, it's me, Alex";

            CloudStorageAccount storageAccount = null;
            CloudBlobContainer cloudBlobContainer = null;

            // Parse the connection string and make sure it's valid
            if (CloudStorageAccount.TryParse(connectionStr, out storageAccount))
            {
                try
                {
                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    // Create a container and append a GUID value to it to make the name unique.
                    cloudBlobContainer = cloudBlobClient.GetContainerReference("allancas-storage-blob-" + "written-text-blobs");
                    await cloudBlobContainer.CreateIfNotExistsAsync();
                    Console.WriteLine("Got reference to/Created container '{0}'", cloudBlobContainer.Name);
                    Console.WriteLine();

                    // Set the permissions so the blobs are public.
                    BlobContainerPermissions permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    await cloudBlobContainer.SetPermissionsAsync(permissions);

                    // Get a reference to the blob address, then upload the text to the blob.
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
                    await cloudBlockBlob.UploadTextAsync(uploadStr);
                }
                catch (StorageException ex)
                {
                    Console.WriteLine("Error returned from the service: {0}", ex.Message);
                }
            }
            else
            {
                Console.WriteLine("ERROR: Connection string did not parse properly");
            }

            return cloudBlobContainer;
        }

        private static async Task<String> ReadBlobText(CloudBlobContainer cloudBlobContainer, string blobName)
        {
            try
            {
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
                return await cloudBlockBlob.DownloadTextAsync();
            }
            catch (StorageException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
            }

            return null;
        }

        private static async Task DeleteBlob(CloudBlobContainer cloudBlobContainer, string blobName)
        {
            try
            {
                Console.WriteLine("Press any key to delete the example blob.");
                Console.ReadLine();

                if (cloudBlobContainer != null)
                {
                    var blob = cloudBlobContainer.GetBlockBlobReference(blobName);
                    await blob.DeleteIfExistsAsync();
                }
            }
            catch (StorageException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
            }
        }

        private static async Task ListBlobsInContainer(CloudBlobContainer cloudBlobContainer, string prefix)
        {
            Console.WriteLine("Listing blobs in container.");
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var results = await cloudBlobContainer.ListBlobsSegmentedAsync(prefix, blobContinuationToken);
                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;
                foreach (IListBlobItem item in results.Results)
                {
                    Console.WriteLine(item.Uri);
                }
            } while (blobContinuationToken != null); // Loop while the continuation token is not null.
        }
    }
}
