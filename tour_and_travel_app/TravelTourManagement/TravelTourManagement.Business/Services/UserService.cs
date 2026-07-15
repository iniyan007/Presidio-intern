using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Users;
using TravelTourManagement.DataAccess.Interface;
using AutoMapper;

namespace TravelTourManagement.Business.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IBlobStorageService _blobStorageService;

    public UserService(IUserRepository userRepository, IMapper mapper, IBlobStorageService blobStorageService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _blobStorageService = blobStorageService;
    }

    public async Task<UserResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithPackagerProfileAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        return _mapper.Map<UserResponse>(user);
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

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> UploadProfilePictureAsync(Guid userId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithPackagerProfileAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        // Delete old picture if exists
        if (!string.IsNullOrEmpty(user.ProfilePicture))
        {
            await _blobStorageService.DeleteFileAsync(user.ProfilePicture, "web-images", cancellationToken);
        }

        // Save new picture
        var fileUrl = await _blobStorageService.UploadFileAsync(fileStream, fileName, contentType, "web-images", cancellationToken);

        user.ProfilePicture = fileUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> RemoveProfilePictureAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithPackagerProfileAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (!string.IsNullOrEmpty(user.ProfilePicture))
        {
            await _blobStorageService.DeleteFileAsync(user.ProfilePicture, "web-images", cancellationToken);
            user.ProfilePicture = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        return _mapper.Map<UserResponse>(user);
    }
}
