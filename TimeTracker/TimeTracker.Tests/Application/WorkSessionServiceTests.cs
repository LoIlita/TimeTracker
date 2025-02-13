using AutoMapper;
using Moq;
using TimeTracker.Application.DTOs;
using TimeTracker.Application.Mapping;
using TimeTracker.Application.Services;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Repositories;

namespace TimeTracker.Tests.Application;

public class WorkSessionServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IWorkSessionRepository> _repositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly IWorkSessionService _service;

    public WorkSessionServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<WorkSessionMappingProfile>());
        _mapper = config.CreateMapper();
        _repositoryMock = new Mock<IWorkSessionRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _service = new WorkSessionService(_repositoryMock.Object, _projectRepositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task StartSessionAsync_ShouldCreateNewSession_WhenNoActiveSession()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetActiveSessionAsync())
            .ReturnsAsync((WorkSession?)null);

        var dto = new CreateWorkSessionDto { Description = "Test session" };

        // Act
        var result = await _service.StartSessionAsync(dto);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(dto.Description, result.Description);
        Assert.True(result.IsActive);
        Assert.Null(result.EndTime);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<WorkSession>()), Times.Once);
    }

    [Fact]
    public async Task StartSessionAsync_ShouldThrowException_WhenActiveSessionExists()
    {
        // Arrange
        var activeSession = WorkSession.Start("Active session");
        _repositoryMock.Setup(r => r.GetActiveSessionAsync())
            .ReturnsAsync(activeSession);

        var dto = new CreateWorkSessionDto { Description = "Test session" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.StartSessionAsync(dto));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<WorkSession>()), Times.Never);
    }

    [Fact]
    public async Task CompleteCurrentSessionAsync_ShouldCompleteSession_WhenActiveSessionExists()
    {
        // Arrange
        var activeSession = WorkSession.Start("Active session");
        _repositoryMock.Setup(r => r.GetActiveSessionAsync())
            .ReturnsAsync(activeSession);

        // Act
        var result = await _service.CompleteCurrentSessionAsync();

        // Assert
        Assert.False(result.IsActive);
        Assert.NotNull(result.EndTime);
        Assert.Equal(activeSession.Id, result.Id);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<WorkSession>()), Times.Once);
    }

    [Fact]
    public async Task CompleteCurrentSessionAsync_ShouldThrowException_WhenNoActiveSession()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetActiveSessionAsync())
            .ReturnsAsync((WorkSession?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CompleteCurrentSessionAsync());

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<WorkSession>()), Times.Never);
    }

    [Fact]
    public async Task GetActiveSessionAsync_ShouldReturnNull_WhenNoActiveSession()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetActiveSessionAsync())
            .ReturnsAsync((WorkSession?)null);

        // Act
        var result = await _service.GetActiveSessionAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveSessionAsync_ShouldReturnSession_WhenActiveSessionExists()
    {
        // Arrange
        var activeSession = WorkSession.Start("Active session");
        _repositoryMock.Setup(r => r.GetActiveSessionAsync())
            .ReturnsAsync(activeSession);

        // Act
        var result = await _service.GetActiveSessionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(activeSession.Id, result.Id);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task UpdateSessionAsync_ShouldUpdateSession_WhenSessionExists()
    {
        // Arrange
        var session = WorkSession.Start("Initial description");
        _repositoryMock.Setup(r => r.GetByIdAsync(session.Id))
            .ReturnsAsync(session);

        var updateDto = new UpdateWorkSessionDto
        {
            Description = "Updated description",
            StartTime = DateTime.Now.AddMinutes(-30),
            EndTime = DateTime.Now.AddMinutes(-10)
        };

        // Act
        var result = await _service.UpdateSessionAsync(session.Id, updateDto);

        // Assert
        Assert.Equal(updateDto.Description, result.Description);
        Assert.Equal(updateDto.StartTime, result.StartTime);
        Assert.Equal(updateDto.EndTime, result.EndTime);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<WorkSession>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSessionAsync_ShouldThrowException_WhenSessionNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((WorkSession?)null);

        var updateDto = new UpdateWorkSessionDto
        {
            Description = "Updated description",
            StartTime = DateTime.Now.AddMinutes(-30),
            EndTime = DateTime.Now.AddMinutes(-10)
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateSessionAsync(id, updateDto));

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<WorkSession>()), Times.Never);
    }
} 