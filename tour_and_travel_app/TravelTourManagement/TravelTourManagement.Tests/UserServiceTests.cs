using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.DTOs.Users;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Tests;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _userRepoMock;
    private Mock<IMapper> _mapperMock;
    private UserService _userService;
    private string _uploadDirectory;

    [SetUp]
    public void Setup()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();

        _userService = new UserService(_userRepoMock.Object, _mapperMock.Object);

        // Determine upload directory based on the logic in UserService
        var currentDirectory = Directory.GetCurrentDirectory();
        var solutionDirectory = Directory.GetParent(currentDirectory)?.FullName ?? currentDirectory;
        _uploadDirectory = Path.Combine(solutionDirectory, "TravelTourManagement.DataAccess", "Uploads", "ProfilePictures");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up any test files created in the upload directory
        if (Directory.Exists(_uploadDirectory))
        {
            var files = Directory.GetFiles(_uploadDirectory, "*_test_*");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }

    // --- GetProfileAsync ---

    [Test]
    public async Task GetProfileAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        _userRepoMock.Setup(x => x.GetWithPackagerProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _userService.GetProfileAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found.");
    }

    [Test]
    public async Task GetProfileAsync_UserFound_ReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, FullName = "Test User" };
        var expectedResponse = new UserResponse(userId, "Test User", "test@test.com", null, null, true, true, false);

        _userRepoMock.Setup(x => x.GetWithPackagerProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserResponse>(user)).Returns(expectedResponse);

        var result = await _userService.GetProfileAsync(userId);

        result.Should().BeEquivalentTo(expectedResponse);
    }

    // --- UpdateProfileAsync ---

    [Test]
    public async Task UpdateProfileAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        _userRepoMock.Setup(x => x.GetWithPackagerProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var request = new UpdateProfileRequest { FullName = "New Name" };
        Func<Task> act = async () => await _userService.UpdateProfileAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found.");
    }

    [Test]
    public async Task UpdateProfileAsync_UserFound_UpdatesAndReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, FullName = "Old Name" };
        var request = new UpdateProfileRequest { FullName = "New Name", Phone = "9876543210" };
        var expectedResponse = new UserResponse(userId, "New Name", "test@test.com", "9876543210", null, true, true, false);

        _userRepoMock.Setup(x => x.GetWithPackagerProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserResponse>(user)).Returns(expectedResponse);

        var result = await _userService.UpdateProfileAsync(userId, request);

        user.FullName.Should().Be("New Name");
        user.Phone.Should().Be("9876543210");
        _userRepoMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        result.Should().BeEquivalentTo(expectedResponse);
    }

    // --- UploadProfilePictureAsync ---

    [Test]
    public async Task UploadProfilePictureAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        _userRepoMock.Setup(x => x.GetWithPackagerProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        using var stream = new MemoryStream();
        Func<Task> act = async () => await _userService.UploadProfilePictureAsync(Guid.NewGuid(), stream, "pic.jpg", "image/jpeg");

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found.");
    }

    [Test]
    public async Task UploadProfilePictureAsync_ValidRequest_DeletesOldAndSavesNew()
    {
        var userId = Guid.NewGuid();
        var oldFileName = $"{userId}_test_old.jpg";
        var oldFilePath = Path.Combine(_uploadDirectory, oldFileName);
        
        // Ensure old file exists physically so we can test deletion
        if (!Directory.Exists(_uploadDirectory)) Directory.CreateDirectory(_uploadDirectory);
        File.WriteAllText(oldFilePath, "dummy old content");

        var user = new User { Id = userId, ProfilePicture = oldFileName };
        var expectedResponse = new UserResponse(userId, "Test User", "test@test.com", null, "new_pic.jpg", true, true, false);

        _userRepoMock.Setup(x => x.GetWithPackagerProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserResponse>(user)).Returns(expectedResponse);

        using var newStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("new content"));
        var result = await _userService.UploadProfilePictureAsync(userId, newStream, "_test_new.jpg", "image/jpeg");

        // Old file should be deleted
        File.Exists(oldFilePath).Should().BeFalse();

        // New profile picture should be set
        user.ProfilePicture.Should().NotBeNull();
        user.ProfilePicture.Should().EndWith(".jpg");
        var newFilePath = Path.Combine(_uploadDirectory, user.ProfilePicture);
        File.Exists(newFilePath).Should().BeTrue();

        _userRepoMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        result.Should().BeEquivalentTo(expectedResponse);

        // Cleanup the newly created file
        File.Delete(newFilePath);
    }

    // --- RemoveProfilePictureAsync ---

    [Test]
    public async Task RemoveProfilePictureAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        _userRepoMock.Setup(x => x.GetWithPackagerProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _userService.RemoveProfilePictureAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found.");
    }

    [Test]
    public async Task RemoveProfilePictureAsync_ValidRequest_RemovesPicture()
    {
        var userId = Guid.NewGuid();
        var oldFileName = $"{userId}_test_remove.jpg";
        var oldFilePath = Path.Combine(_uploadDirectory, oldFileName);
        
        // Ensure old file exists physically so we can test deletion
        if (!Directory.Exists(_uploadDirectory)) Directory.CreateDirectory(_uploadDirectory);
        File.WriteAllText(oldFilePath, "dummy old content");

        var user = new User { Id = userId, ProfilePicture = oldFileName };
        var expectedResponse = new UserResponse(userId, "Test User", "test@test.com", null, null, true, true, false);

        _userRepoMock.Setup(x => x.GetWithPackagerProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserResponse>(user)).Returns(expectedResponse);

        var result = await _userService.RemoveProfilePictureAsync(userId);

        // Old file should be deleted
        File.Exists(oldFilePath).Should().BeFalse();

        // Profile picture should be null
        user.ProfilePicture.Should().BeNull();

        _userRepoMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        result.Should().BeEquivalentTo(expectedResponse);
    }
}
