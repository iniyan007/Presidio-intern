using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using Azure.Storage.Blobs;

namespace TourMate.ImageProcessor
{
    public class ImageConverterFunction
    {
        private readonly ILogger<ImageConverterFunction> _logger;

        public ImageConverterFunction(ILogger<ImageConverterFunction> logger)
        {
            _logger = logger;
        }

        [Function("ConvertImageToWebP")]
        public async Task Run(
            [BlobTrigger("raw-images/{name}", Connection = "AzureWebJobsStorage")] Stream inputStream, 
            string name,
            [BlobInput("raw-images", Connection = "AzureWebJobsStorage")] BlobContainerClient inputContainer,
            [BlobInput("web-images", Connection = "AzureWebJobsStorage")] BlobContainerClient outputContainer)
        {
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {inputStream.Length} Bytes");

            try
            {
                // Only process JPG and PNG files
                var extension = Path.GetExtension(name).ToLowerInvariant();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                {
                    _logger.LogInformation($"Skipping file {name} as it is not a JPG or PNG.");
                    return;
                }

                var newName = Path.GetFileNameWithoutExtension(name) + ".webp";
                var blobClient = outputContainer.GetBlobClient(newName);

                using (var memoryStream = new MemoryStream())
                {
                    await inputStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    
                    using (var bitmap = SKBitmap.Decode(memoryStream))
                    {
                        if (bitmap == null)
                        {
                            _logger.LogError($"Failed to decode image {name}");
                            return;
                        }

                        using (var outStream = new MemoryStream())
                        {
                            // Encode as WebP with 80% quality
                            bitmap.Encode(outStream, SKEncodedImageFormat.Webp, 80);
                            outStream.Position = 0;

                            // Upload the compressed webp image
                            await blobClient.UploadAsync(outStream, overwrite: true);
                            _logger.LogInformation($"Successfully converted and uploaded {newName} to web-images container.");
                        }
                    }
                }
                
                // Delete the original uncompressed image to save storage costs
                var inputBlobClient = inputContainer.GetBlobClient(name);
                await inputBlobClient.DeleteIfExistsAsync();
                _logger.LogInformation($"Successfully deleted original file {name} from raw-images container.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing image {name}: {ex.Message}");
                throw;
            }
        }
    }
}
