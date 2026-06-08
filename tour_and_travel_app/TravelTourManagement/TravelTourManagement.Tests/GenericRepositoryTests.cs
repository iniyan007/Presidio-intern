using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Repository;

namespace TravelTourManagement.Tests;

[TestFixture]
public class GenericRepositoryTests
{
    private ApplicationDbContext _dbContext;
    private GenericRepository<User, Guid> _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new GenericRepository<User, Guid>(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private User CreateTestUser(string name = "Test User")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = name,
            Email = $"{name.Replace(" ", "").ToLower()}@test.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Test]
    public async Task GetByIdAsync_ExistingId_ReturnsEntity()
    {
        // Arrange
        var user = CreateTestUser();
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.FullName.Should().Be(user.FullName);
    }

    [Test]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetAllAsync_PopulatedDb_ReturnsAllEntities()
    {
        // Arrange
        var user1 = CreateTestUser("Alice");
        var user2 = CreateTestUser("Bob");
        await _dbContext.Users.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.FullName == "Alice");
        result.Should().Contain(u => u.FullName == "Bob");
    }

    [Test]
    public async Task GetAllAsync_EmptyDb_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task FindAsync_MatchingPredicate_ReturnsEntities()
    {
        // Arrange
        var activeUser = CreateTestUser("Active");
        activeUser.IsActive = true;

        var inactiveUser = CreateTestUser("Inactive");
        inactiveUser.IsActive = false;

        await _dbContext.Users.AddRangeAsync(activeUser, inactiveUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(u => u.IsActive);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().FullName.Should().Be("Active");
    }

    [Test]
    public async Task FindAsync_NonMatchingPredicate_ReturnsEmpty()
    {
        // Arrange
        var user = CreateTestUser("Alice");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(u => u.FullName == "Bob");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task AddAsync_ValidEntity_AddsAndSaves()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        dbUser.Should().NotBeNull();
        dbUser!.FullName.Should().Be(user.FullName);
    }

    [Test]
    public async Task UpdateAsync_ValidEntity_UpdatesAndSaves()
    {
        // Arrange
        var user = CreateTestUser();
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        user.FullName = "Updated Name";
        var result = await _repository.UpdateAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("Updated Name");

        var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        dbUser.Should().NotBeNull();
        dbUser!.FullName.Should().Be("Updated Name");
    }

    [Test]
    public async Task DeleteAsync_ExistingId_DeletesAndSaves()
    {
        // Arrange
        var user = CreateTestUser();
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(user.Id);

        // Assert
        var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        dbUser.Should().BeNull();
    }

    [Test]
    public async Task DeleteAsync_NonExistingId_DoesNothingGracefully()
    {
        // Arrange
        var user = CreateTestUser();
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
        var totalUsers = await _dbContext.Users.CountAsync();
        totalUsers.Should().Be(1); // The existing user was not deleted
    }

    [Test]
    public async Task ExistsAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestUser();
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(user.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_NonExistingId_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task CountAsync_NoPredicate_ReturnsTotalCount()
    {
        // Arrange
        await _dbContext.Users.AddRangeAsync(CreateTestUser("A"), CreateTestUser("B"), CreateTestUser("C"));
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Test]
    public async Task CountAsync_WithPredicate_ReturnsFilteredCount()
    {
        // Arrange
        var user1 = CreateTestUser("A");
        user1.IsActive = true;

        var user2 = CreateTestUser("B");
        user2.IsActive = false;

        await _dbContext.Users.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync(u => u.IsActive);

        // Assert
        result.Should().Be(1);
    }
}
