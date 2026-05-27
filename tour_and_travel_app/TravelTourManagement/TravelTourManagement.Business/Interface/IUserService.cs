using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Users;

namespace TravelTourManagement.Business.Interface;

public interface IUserService
{
    Task<UserResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> UploadProfilePictureAsync(Guid userId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<UserResponse> RemoveProfilePictureAsync(Guid userId, CancellationToken cancellationToken = default);
}
