using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace UploadFileAzure
{
    public class Storage
    {
        private readonly BlobServiceClient _blobServiceClient;

        public Storage(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<bool> UploadNewBlobAsync(
            string containerName,
            string blobName,
            Stream stream,
            CancellationToken cancellationToken = default)
        {

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var options = new BlobUploadOptions
            {
                Conditions = new BlobRequestConditions
                {
                    IfNoneMatch = ETag.All
                }
            };

            try
            {
                _ = await blobClient.UploadAsync(stream, options, cancellationToken);
            }
            catch (RequestFailedException exception) when (IsConflict(exception))
            {
                return false;
            }

            return true;
        }

        public async Task<BlobDownloadInfo> GetBlobAsync(
            string containerName,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                var response = await blobClient.DownloadAsync(cancellationToken);
                return response.Value;
            }
            catch (RequestFailedException exception) when (IsNotFound(exception))
            {
                return null;
            }
        }

        public async Task<List<string>> ListBlobsInContainerAsync(
            string containerName,
            CancellationToken cancellationToken)
        {
            List<string> stringList = new List<string>();
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync().WithCancellation(cancellationToken))
            {
                stringList.Add(blobItem.Name);
            }

            return stringList;
        }
        private static bool IsConflict(RequestFailedException exception) =>
            exception.Status == (int)HttpStatusCode.Conflict ||
            exception.Status == (int)HttpStatusCode.PreconditionFailed;

        private static bool IsNotFound(RequestFailedException exception) =>
            exception.Status == (int)HttpStatusCode.NotFound;
    }
}
