using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace UploadFileAzure
{
    public static class FileProcessFunctionUpload
    {
        [FunctionName("FileProcessFunctionUpload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, CancellationToken cancellationToken)
        {
            log.LogInformation("Upload started");

            try
            {
                var formdata = await req.ReadFormAsync();
                var stream = formdata.Files.GetFile("file").OpenReadStream();
                var file = req.Form.Files["file"];

                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var blobServiceClient = new BlobServiceClient(connectionString);
                var storage = new Storage(blobServiceClient);

                await storage.UploadNewBlobAsync("files", file.FileName, stream, cancellationToken);

                var list = await storage.ListBlobsInContainerAsync("files", cancellationToken);

                return new OkObjectResult(list);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    };
}

