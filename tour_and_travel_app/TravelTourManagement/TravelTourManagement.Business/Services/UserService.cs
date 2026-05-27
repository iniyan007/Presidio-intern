using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Users;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Business.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly string _uploadDirectory;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
        // Define path in DataAccess layer (as requested by user)
        var currentDirectory = Directory.GetCurrentDirectory(); 
        var solutionDirectory = Directory.GetParent(currentDirectory)?.FullName ?? currentDirectory;
        _uploadDirectory = Path.Combine(solutionDirectory, "TravelTourManagement.DataAccess", "Uploads", "ProfilePictures");
        
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }
    }

    public async Task<UserResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithPackagerProfileAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.ProfilePicture,
            user.IsActive,
            user.IsEmailVerified,
            user.PackagerUser != null && user.PackagerUser.ApprovedAt != null && user.PackagerUser.DeactivatedAt == null
        );
    }

    public async Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithPackagerProfileAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.ProfilePicture,
            user.IsActive,
            user.IsEmailVerified,
            user.PackagerUser != null && user.PackagerUser.ApprovedAt != null && user.PackagerUser.DeactivatedAt == null
        );
    }

    public async Task<UserResponse> UploadProfilePictureAsync(Guid userId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithPackagerProfileAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        // Generate unique file name
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_uploadDirectory, uniqueFileName);

        // Delete old picture if exists
        if (!string.IsNullOrEmpty(user.ProfilePicture))
        {
            var oldFilePath = Path.Combine(_uploadDirectory, user.ProfilePicture);
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }
        }

        // Save new picture
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream, cancellationToken);
        }

        user.ProfilePicture = uniqueFileName;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.ProfilePicture,
            user.IsActive,
            user.IsEmailVerified,
            user.PackagerUser != null && user.PackagerUser.ApprovedAt != null && user.PackagerUser.DeactivatedAt == null
        );
    }

    public async Task<UserResponse> RemoveProfilePictureAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithPackagerProfileAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (!string.IsNullOrEmpty(user.ProfilePicture))
        {
            var oldFilePath = Path.Combine(_uploadDirectory, user.ProfilePicture);
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }
            user.ProfilePicture = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.ProfilePicture,
            user.IsActive,
            user.IsEmailVerified,
            user.PackagerUser != null && user.PackagerUser.ApprovedAt != null && user.PackagerUser.DeactivatedAt == null
        );
    }
}
