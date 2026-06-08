using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.DTOs.Messages;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.Tests;

[TestFixture]
public class MessageServiceTests
{
    private ApplicationDbContext _dbContext;
    private Mock<IMessageDispatcher> _dispatcherMock;
    private MessageService _messageService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dispatcherMock = new Mock<IMessageDispatcher>();

        _messageService = new MessageService(_dbContext, _dispatcherMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    // --- GetOrInitializeThreadAsync ---

    [Test]
    public async Task GetOrInitializeThreadAsync_PackagerNotFound_ThrowsKeyNotFoundException()
    {
        Func<Task> act = async () => await _messageService.GetOrInitializeThreadAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Packager not found.");
    }

    [Test]
    public async Task GetOrInitializeThreadAsync_ThreadDoesNotExist_CreatesAndReturnsNewThread()
    {
        var userId = Guid.NewGuid();
        var packagerId = Guid.NewGuid();
        var packageId = Guid.NewGuid();

        await _dbContext.Users.AddAsync(new User { Id = userId, FullName = "Test User", Email = "test@test.com", PasswordHash = "hash" });
        var packagerUser = new User { Id = Guid.NewGuid(), FullName = "Packager User", Email = "packager@test.com", PasswordHash = "hash" };
        await _dbContext.Users.AddAsync(packagerUser);
        await _dbContext.Packagers.AddAsync(new Packager { Id = packagerId, UserId = packagerUser.Id, CompanyName = "Company" });
        await _dbContext.Packages.AddAsync(new Package { Id = packageId, Title = "Test Package", Destination = "Dest", Country = "Country" });
        await _dbContext.SaveChangesAsync();

        var threadDto = await _messageService.GetOrInitializeThreadAsync(userId, packagerId, packageId);

        threadDto.Should().NotBeNull();
        threadDto.UserId.Should().Be(userId);
        threadDto.PackagerId.Should().Be(packagerId);
        threadDto.PackageId.Should().Be(packageId);

        var dbThread = await _dbContext.MessageThreads.FirstOrDefaultAsync();
        dbThread.Should().NotBeNull();
    }

    [Test]
    public async Task GetOrInitializeThreadAsync_ThreadExists_ReturnsExistingThread()
    {
        var userId = Guid.NewGuid();
        var packagerId = Guid.NewGuid();
        var packageId = Guid.NewGuid();

        var user = new User { Id = userId, FullName = "Test User", Email = "u1@t.com", PasswordHash = "hash" };
        var packagerUser = new User { Id = Guid.NewGuid(), FullName = "Packager User", Email = "p1@t.com", PasswordHash = "hash" };
        var packager = new Packager { Id = packagerId, UserId = packagerUser.Id, CompanyName = "Company" };
        var package = new Package { Id = packageId, Title = "Test Package", Destination = "Dest", Country = "Country" };
        
        var existingThread = new MessageThread
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PackagerId = packagerId,
            PackageId = packageId,
            User = user,
            Packager = packager,
            Package = package,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Users.AddAsync(packagerUser);
        await _dbContext.Packagers.AddAsync(packager);
        await _dbContext.Packages.AddAsync(package);
        await _dbContext.MessageThreads.AddAsync(existingThread);
        await _dbContext.SaveChangesAsync();

        var threadDto = await _messageService.GetOrInitializeThreadAsync(userId, packagerId, packageId);

        threadDto.Should().NotBeNull();
        threadDto.Id.Should().Be(existingThread.Id);
        
        var threadCount = await _dbContext.MessageThreads.CountAsync();
        threadCount.Should().Be(1);
    }

    // --- SendMessageAsync ---

    [Test]
    public async Task SendMessageAsync_ThreadNotFound_ThrowsKeyNotFoundException()
    {
        var request = new SendMessageRequest { ThreadId = Guid.NewGuid(), Body = "Hello", SenderRole = MessageSenderRole.user };
        Func<Task> act = async () => await _messageService.SendMessageAsync(Guid.NewGuid(), request);
        
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Chat thread not found.");
    }

    [Test]
    public async Task SendMessageAsync_UserNotPartOfThread_ThrowsUnauthorizedAccessException()
    {
        var packager = new Packager { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CompanyName = "Comp" };
        var thread = new MessageThread { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PackagerId = packager.Id, Packager = packager };
        await _dbContext.Packagers.AddAsync(packager);
        await _dbContext.MessageThreads.AddAsync(thread);
        await _dbContext.SaveChangesAsync();

        var request = new SendMessageRequest { ThreadId = thread.Id, Body = "Hello", SenderRole = MessageSenderRole.user };
        Func<Task> act = async () => await _messageService.SendMessageAsync(Guid.NewGuid(), request);
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You are not part of this chat thread.");
    }

    [Test]
    public async Task SendMessageAsync_PackagerNotPartOfThread_ThrowsUnauthorizedAccessException()
    {
        var packager = new Packager { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CompanyName = "Comp" };
        var thread = new MessageThread { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PackagerId = packager.Id, Packager = packager };
        await _dbContext.Packagers.AddAsync(packager);
        await _dbContext.MessageThreads.AddAsync(thread);
        await _dbContext.SaveChangesAsync();

        var request = new SendMessageRequest { ThreadId = thread.Id, Body = "Hello", SenderRole = MessageSenderRole.packager };
        Func<Task> act = async () => await _messageService.SendMessageAsync(Guid.NewGuid(), request);
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You are not part of this chat thread.");
    }

    [Test]
    public async Task SendMessageAsync_ValidRequest_SavesMessageAndDispatches()
    {
        var userId = Guid.NewGuid();
        var packagerUserId = Guid.NewGuid();
        var packager = new Packager { Id = Guid.NewGuid(), UserId = packagerUserId, CompanyName = "Comp" };
        var thread = new MessageThread { Id = Guid.NewGuid(), UserId = userId, PackagerId = packager.Id, Packager = packager };
        
        await _dbContext.Packagers.AddAsync(packager);
        await _dbContext.MessageThreads.AddAsync(thread);
        await _dbContext.SaveChangesAsync();

        var request = new SendMessageRequest { ThreadId = thread.Id, Body = "Hello World", SenderRole = MessageSenderRole.user };
        
        var result = await _messageService.SendMessageAsync(userId, request);

        result.Should().NotBeNull();
        result.Body.Should().Be("Hello World");
        
        var dbMessage = await _dbContext.Messages.FirstOrDefaultAsync();
        dbMessage.Should().NotBeNull();
        dbMessage!.Body.Should().Be("Hello World");
        
        var dbThread = await _dbContext.MessageThreads.FindAsync(thread.Id);
        dbThread!.LastMessageAt.Should().Be(dbMessage.SentAt);

        _dispatcherMock.Verify(d => d.DispatchMessageAsync(userId, packagerUserId, It.Is<MessageDto>(m => m.Body == "Hello World"), It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- GetThreadMessagesAsync ---

    [Test]
    public async Task GetThreadMessagesAsync_ThreadNotFound_ThrowsKeyNotFoundException()
    {
        Func<Task> act = async () => await _messageService.GetThreadMessagesAsync(Guid.NewGuid(), Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Chat thread not found.");
    }

    [Test]
    public async Task GetThreadMessagesAsync_RequestorNotPartOfThread_ThrowsUnauthorizedAccessException()
    {
        var packager = new Packager { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CompanyName = "Comp" };
        var thread = new MessageThread { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PackagerId = packager.Id, Packager = packager };
        await _dbContext.Packagers.AddAsync(packager);
        await _dbContext.MessageThreads.AddAsync(thread);
        await _dbContext.SaveChangesAsync();

        Func<Task> act = async () => await _messageService.GetThreadMessagesAsync(thread.Id, Guid.NewGuid());
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You are not part of this chat thread.");
    }

    [Test]
    public async Task GetThreadMessagesAsync_ValidRequest_ReturnsMessagesOrdered()
    {
        var userId = Guid.NewGuid();
        var packager = new Packager { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CompanyName = "Comp" };
        var thread = new MessageThread { Id = Guid.NewGuid(), UserId = userId, PackagerId = packager.Id, Packager = packager };
        
        await _dbContext.Packagers.AddAsync(packager);
        await _dbContext.MessageThreads.AddAsync(thread);
        
        var messages = new List<Message>
        {
            new Message { Id = Guid.NewGuid(), ThreadId = thread.Id, SentAt = DateTime.UtcNow.AddMinutes(2), Body = "Second" },
            new Message { Id = Guid.NewGuid(), ThreadId = thread.Id, SentAt = DateTime.UtcNow.AddMinutes(1), Body = "First" }
        };
        await _dbContext.Messages.AddRangeAsync(messages);
        await _dbContext.SaveChangesAsync();

        var result = await _messageService.GetThreadMessagesAsync(thread.Id, userId);

        result.Should().HaveCount(2);
        result.First().Body.Should().Be("First");
        result.Last().Body.Should().Be("Second");
    }

    // --- GetUserThreadsAsync ---

    [Test]
    public async Task GetUserThreadsAsync_User_ReturnsThreadsWithUnreadCounts()
    {
        var userId = Guid.NewGuid();
        var packagerUser = new User { Id = Guid.NewGuid(), FullName = "P", Email = "p@t.com", PasswordHash = "hash" };
        var packager = new Packager { Id = Guid.NewGuid(), UserId = packagerUser.Id, User = packagerUser, CompanyName = "Comp" };
        var user = new User { Id = userId, FullName = "U", Email = "u@t.com", PasswordHash = "hash" };
        
        await _dbContext.Packagers.AddAsync(packager);

        var thread1 = new MessageThread { Id = Guid.NewGuid(), UserId = userId, User = user, Packager = packager, PackagerId = packager.Id, LastMessageAt = DateTime.UtcNow };
        var thread2 = new MessageThread { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), FullName = "Other", Email = "o@t.com", PasswordHash = "h" }, Packager = packager, PackagerId = packager.Id, LastMessageAt = DateTime.UtcNow }; // Different user
        
        await _dbContext.MessageThreads.AddRangeAsync(thread1, thread2);
        
        var messages = new List<Message>
        {
            new Message { Id = Guid.NewGuid(), ThreadId = thread1.Id, SenderRole = MessageSenderRole.packager, IsRead = false, SentAt = DateTime.UtcNow, Body = "msg" }, // Unread for user
            new Message { Id = Guid.NewGuid(), ThreadId = thread1.Id, SenderRole = MessageSenderRole.user, IsRead = false, SentAt = DateTime.UtcNow, Body = "msg" } // Unread for packager (should not count for user)
        };
        await _dbContext.Messages.AddRangeAsync(messages);
        await _dbContext.SaveChangesAsync();

        var result = (await _messageService.GetUserThreadsAsync(userId, isPackager: false)).ToList();

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(thread1.Id);
        result.First().UnreadCount.Should().Be(1);
        result.First().LastMessage.Should().NotBeNull();
    }

    // --- MarkMessagesAsReadAsync ---

    [Test]
    public async Task MarkMessagesAsReadAsync_ThreadNotFound_ReturnsFalse()
    {
        var result = await _messageService.MarkMessagesAsReadAsync(Guid.NewGuid(), Guid.NewGuid(), MessageSenderRole.user);
        result.Should().BeFalse();
    }

    [Test]
    public async Task MarkMessagesAsReadAsync_UnauthorizedUser_ReturnsFalse()
    {
        var packager = new Packager { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CompanyName = "Comp" };
        var thread = new MessageThread { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PackagerId = packager.Id, Packager = packager };
        await _dbContext.Packagers.AddAsync(packager);
        await _dbContext.MessageThreads.AddAsync(thread);
        await _dbContext.SaveChangesAsync();

        var result = await _messageService.MarkMessagesAsReadAsync(thread.Id, Guid.NewGuid(), MessageSenderRole.user);
        result.Should().BeFalse();
    }

    [Test]
    public async Task MarkMessagesAsReadAsync_ValidRequest_MarksOtherPersonsMessagesAsRead()
    {
        var userId = Guid.NewGuid();
        var packager = new Packager { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CompanyName = "Comp" };
        var thread = new MessageThread { Id = Guid.NewGuid(), UserId = userId, PackagerId = packager.Id, Packager = packager };
        
        await _dbContext.Packagers.AddAsync(packager);
        await _dbContext.MessageThreads.AddAsync(thread);
        
        var messages = new List<Message>
        {
            new Message { Id = Guid.NewGuid(), ThreadId = thread.Id, SenderRole = MessageSenderRole.packager, IsRead = false, SentAt = DateTime.UtcNow, Body = "msg" }, // Unread from packager
            new Message { Id = Guid.NewGuid(), ThreadId = thread.Id, SenderRole = MessageSenderRole.user, IsRead = false, SentAt = DateTime.UtcNow, Body = "msg" } // Unread from user
        };
        await _dbContext.Messages.AddRangeAsync(messages);
        await _dbContext.SaveChangesAsync();

        // User is requesting to mark messages as read. So it should mark packager's messages as read.
        var result = await _messageService.MarkMessagesAsReadAsync(thread.Id, userId, MessageSenderRole.user);

        result.Should().BeTrue();
        
        var unreadFromPackager = await _dbContext.Messages.CountAsync(m => m.SenderRole == MessageSenderRole.packager && !m.IsRead);
        var unreadFromUser = await _dbContext.Messages.CountAsync(m => m.SenderRole == MessageSenderRole.user && !m.IsRead);

        unreadFromPackager.Should().Be(0); // Marked as read
        unreadFromUser.Should().Be(1); // Left alone
    }
}
