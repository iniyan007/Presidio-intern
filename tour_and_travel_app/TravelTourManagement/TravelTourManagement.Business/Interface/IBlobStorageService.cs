using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TravelTourManagement.Business.Interface;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileUrl, string containerName, CancellationToken cancellationToken = default);
    Task<(Stream Content, string ContentType)> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}
