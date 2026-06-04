using Azure.Storage.Blobs;

namespace EventEase.Services
{
    public class BlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public BlobService(IConfiguration config)
        {
            _connectionString = config["AzureStorage:ConnectionString"];
            _containerName = config["AzureStorage:ContainerName"];
        }

        public async Task<string> UploadFileAsync(IFormFile file)       
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            await containerClient.CreateIfNotExistsAsync();

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString(); // THIS IS THE IMAGE URL
        }
    }
}