using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using TravelTourManagement.Business.Interface;

namespace TravelTourManagement.Business.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly string _connectionString;

    public BlobStorageService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AzureWebJobsStorage") ?? string.Empty;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Azure Blob Storage connection string is not configured.");
        }

        var blobServiceClient = new BlobServiceClient(_connectionString);
        var targetContainerName = containerName;
        
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        bool isConvertibleImage = containerName == "web-images" && (extension == ".jpg" || extension == ".jpeg" || extension == ".png");

        if (isConvertibleImage)
        {
            targetContainerName = "raw-images";
        }

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(targetContainerName);

        // Ensure container exists
        await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        // Use a unique name to prevent overwriting
        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);

        var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
        
        fileStream.Position = 0; // Ensure stream is at the beginning
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders }, cancellationToken);

        string uploadedUrl = blobClient.Uri.ToString();

        if (isConvertibleImage)
        {
            // Predict the converted WebP URL in the web-images container
            var predictedFileName = Path.GetFileNameWithoutExtension(uniqueFileName) + ".webp";
            var outputContainerClient = blobServiceClient.GetBlobContainerClient("web-images");
            var predictedBlobClient = outputContainerClient.GetBlobClient(predictedFileName);
            return predictedBlobClient.Uri.ToString();
        }

        return uploadedUrl;
    }

    public async Task DeleteFileAsync(string fileUrl, string containerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_connectionString) || string.IsNullOrEmpty(fileUrl))
        {
            return;
        }

        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        var uri = new Uri(fileUrl);
        var blobName = Path.GetFileName(uri.LocalPath);

        var blobClient = blobContainerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }
}
