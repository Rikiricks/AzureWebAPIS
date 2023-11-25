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

    }
}
