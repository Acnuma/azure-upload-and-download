using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UploadFileAzure
{
    class FileProcessFunctionDownload
    {
        [FunctionName("FileProcessFunctionDownload")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
             ILogger log, CancellationToken cancellationToken)
        {
            try
            {
                string file = req.Query["file"];

                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var blobServiceClient = new BlobServiceClient(connectionString);
                var storage = new Storage(blobServiceClient);

                var info = await storage.GetBlobAsync("files", file, cancellationToken);
                
                return new FileStreamResult(info.Content, info.ContentType) {
                    FileDownloadName = file,
                };
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    };
}
