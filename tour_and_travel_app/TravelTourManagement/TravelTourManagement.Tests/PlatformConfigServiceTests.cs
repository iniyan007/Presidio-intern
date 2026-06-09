using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NUnit.Framework;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.DTOs.PlatformConfig;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Tests;

[TestFixture]
public class PlatformConfigServiceTests
{
    private Mock<IRepository<PlatformConfig, Guid>> _repoMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IDistributedCache> _cacheMock;
    private PlatformConfigService _configService;

    [SetUp]
    public void Setup()
    {
        _repoMock = new Mock<IRepository<PlatformConfig, Guid>>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IDistributedCache>();

        // Mock GetAsync to return null by default so it falls back to DB
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _configService = new PlatformConfigService(_repoMock.Object, _mapperMock.Object, _cacheMock.Object);
    }

    [Test]
    public async Task GetConfigAsync_NoConfigExists_CreatesDefaultAndReturns()
    {
        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PlatformConfig>());

        var response = new PlatformConfigResponse { PlatformFeePercent = 5.0m, GstPercent = 10.0m, UpdatedAt = DateTime.UtcNow, Note = "Default" };
        _mapperMock.Setup(x => x.Map<PlatformConfigResponse>(It.IsAny<PlatformConfig>())).Returns(response);

        var result = await _configService.GetConfigAsync();

        _repoMock.Verify(x => x.AddAsync(It.Is<PlatformConfig>(c => c.PlatformFeePercent == 5.0m && c.GstPercent == 10.0m), It.IsAny<CancellationToken>()), Times.Once);
        result.Should().BeEquivalentTo(response);
    }

    [Test]
    public async Task GetConfigAsync_ConfigExists_ReturnsExisting()
    {
        var existingConfig = new PlatformConfig { PlatformFeePercent = 12.0m, GstPercent = 18.0m };
        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PlatformConfig> { existingConfig });

        var response = new PlatformConfigResponse { PlatformFeePercent = 12.0m, GstPercent = 18.0m, UpdatedAt = DateTime.UtcNow, Note = "Existing" };
        _mapperMock.Setup(x => x.Map<PlatformConfigResponse>(existingConfig)).Returns(response);

        var result = await _configService.GetConfigAsync();

        _repoMock.Verify(x => x.AddAsync(It.IsAny<PlatformConfig>(), It.IsAny<CancellationToken>()), Times.Never);
        result.Should().BeEquivalentTo(response);
    }

    [Test]
    public async Task UpdateConfigAsync_NoConfigExists_CreatesNew()
    {
        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PlatformConfig>());

        var adminId = Guid.NewGuid();
        var request = new UpdatePlatformConfigRequest { PlatformFeePercent = 7.5m, GstPercent = 12.5m, Note = "New config" };
        var response = new PlatformConfigResponse { PlatformFeePercent = 7.5m, GstPercent = 12.5m, UpdatedAt = DateTime.UtcNow, UpdatedBy = adminId, Note = "New config" };
        
        _mapperMock.Setup(x => x.Map<PlatformConfigResponse>(It.IsAny<PlatformConfig>())).Returns(response);

        var result = await _configService.UpdateConfigAsync(adminId, request);

        _repoMock.Verify(x => x.AddAsync(It.Is<PlatformConfig>(c => c.PlatformFeePercent == 7.5m && c.UpdatedBy == adminId && c.Note == "New config"), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<PlatformConfig>(), It.IsAny<CancellationToken>()), Times.Never);
        
        result.Should().BeEquivalentTo(response);
    }

    [Test]
    public async Task UpdateConfigAsync_ConfigExists_UpdatesExisting()
    {
        var existingConfig = new PlatformConfig { PlatformFeePercent = 5.0m, GstPercent = 10.0m };
        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PlatformConfig> { existingConfig });

        var adminId = Guid.NewGuid();
        var request = new UpdatePlatformConfigRequest { PlatformFeePercent = 7.5m, GstPercent = 12.5m, Note = "Updated config" };
        var response = new PlatformConfigResponse { PlatformFeePercent = 7.5m, GstPercent = 12.5m, UpdatedAt = DateTime.UtcNow, UpdatedBy = adminId, Note = "Updated config" };
        
        _mapperMock.Setup(x => x.Map<PlatformConfigResponse>(existingConfig)).Returns(response);

        var result = await _configService.UpdateConfigAsync(adminId, request);

        existingConfig.PlatformFeePercent.Should().Be(7.5m);
        existingConfig.GstPercent.Should().Be(12.5m);
        existingConfig.UpdatedBy.Should().Be(adminId);
        existingConfig.Note.Should().Be("Updated config");

        _repoMock.Verify(x => x.AddAsync(It.IsAny<PlatformConfig>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(x => x.UpdateAsync(existingConfig, It.IsAny<CancellationToken>()), Times.Once);
        
        result.Should().BeEquivalentTo(response);
    }
}
