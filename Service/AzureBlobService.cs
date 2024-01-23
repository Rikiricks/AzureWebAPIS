using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AspNetRestAPI.Service
{
    public class AzureBlobService
    {
        private BlobServiceClient _blobClient;
        private BlobContainerClient _containerClient;

        public AzureBlobService(string connectionStr, string containerName)
        {
            _blobClient = new BlobServiceClient(connectionStr);
            _containerClient = _blobClient.GetBlobContainerClient(containerName);
        }

        public async Task<List<Azure.Response<BlobContentInfo>>> UploadFiles(List<IFormFile> files)
        {
            var azureResponse = new List<Azure.Response<BlobContentInfo>>(0);
            foreach (var file in files)
            {
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    var client = await _containerClient.UploadBlobAsync(file.FileName, memoryStream,default);
                    azureResponse.Add(client);
                }
            }
            return azureResponse;
        }

        public async Task<List<BlobItem>> GetUploadedBlobs()
        {
            var items = new List<BlobItem>();
            var uploadedFiles = _containerClient.GetBlobsAsync();
            await foreach (BlobItem file in uploadedFiles)
            {
                items.Add(file);
            }

            return items;
        }

        private  async Task ReadContainerPropertiesAsync(BlobContainerClient container)
        {
            try
            {
                // Fetch some container properties and write out their values.
                var properties = await _containerClient.GetPropertiesAsync();
                Console.WriteLine($"Properties for container {container.Uri}");
                Console.WriteLine($"Public access level: {properties.Value.PublicAccess}");
                Console.WriteLine($"Last modified time in UTC: {properties.Value.LastModified}");
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }

        public async Task AddContainerMetadataAsync(BlobContainerClient container)
        {
            try
            {
                IDictionary<string, string> metadata =
                   new Dictionary<string, string>();

                // Add some metadata to the container.
                metadata.Add("docType", "textDocuments");
                metadata.Add("category", "guidance");

                // Set the container's metadata.
                await container.SetMetadataAsync(metadata);
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }

        public async Task ReadContainerMetadataAsync(BlobContainerClient container)
        {
            try
            {
                var properties = await container.GetPropertiesAsync();

                // Enumerate the container's metadata.
                Console.WriteLine("Container metadata:");
                foreach (var metadataItem in properties.Value.Metadata)
                {
                    Console.WriteLine($"\tKey: {metadataItem.Key}");
                    Console.WriteLine($"\tValue: {metadataItem.Value}");
                }
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }

    }
}
