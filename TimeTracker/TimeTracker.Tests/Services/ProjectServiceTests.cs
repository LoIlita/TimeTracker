using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using TimeTracker.Application.DTOs;
using TimeTracker.Application.Services;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Repositories;
using TimeTracker.Domain.Validation;

namespace TimeTracker.Tests.Services;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IWorkSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ProjectService>> _loggerMock;
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _sessionRepositoryMock = new Mock<IWorkSessionRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ProjectService>>();

        _service = new ProjectService(
            _projectRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProjectDto_WhenProjectExists()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = Project.Create("Test Project", ProjectType.Work);
        var expectedDto = new ProjectDto { Id = projectId, Name = "Test Project" };

        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId))
            .ReturnsAsync(project);
        _mapperMock.Setup(m => m.Map<ProjectDto>(project))
            .Returns(expectedDto);

        // Act
        var result = await _service.GetByIdAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowKeyNotFoundException_WhenProjectDoesNotExist()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(projectId));
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnProjectDto_WhenDataIsValid()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            Name = "New Project",
            Type = ProjectType.Work,
            Description = "Test Description"
        };

        var project = Project.Create(createDto.Name, createDto.Type, createDto.Description);
        var expectedDto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Type = project.Type,
            Description = project.Description
        };

        _projectRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Project>()))
            .Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<ProjectDto>(It.IsAny<Project>()))
            .Returns(expectedDto);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Name, result.Name);
        Assert.Equal(expectedDto.Type, result.Type);
        Assert.Equal(expectedDto.Description, result.Description);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenDataIsInvalid()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            Name = "", // Pusta nazwa powinna spowodować błąd walidacji
            Type = ProjectType.Work
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedProjectDto_WhenDataIsValid()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = Project.Create("Old Name", ProjectType.Work);
        var updateDto = new UpdateProjectDto
        {
            Name = "Updated Name",
            Type = ProjectType.Learning,
            Description = "Updated Description"
        };

        var expectedDto = new ProjectDto
        {
            Id = projectId,
            Name = updateDto.Name,
            Type = updateDto.Type,
            Description = updateDto.Description
        };

        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId))
            .ReturnsAsync(project);
        _projectRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Project>()))
            .Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<ProjectDto>(It.IsAny<Project>()))
            .Returns(expectedDto);

        // Act
        var result = await _service.UpdateAsync(projectId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Name, result.Name);
        Assert.Equal(expectedDto.Type, result.Type);
        Assert.Equal(expectedDto.Description, result.Description);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProject_WhenProjectExists()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _projectRepositoryMock.Setup(r => r.DeleteAsync(projectId))
            .Returns(Task.CompletedTask);

        // Act & Assert
        await _service.DeleteAsync(projectId);
        _projectRepositoryMock.Verify(r => r.DeleteAsync(projectId), Times.Once);
    }

    [Fact]
    public async Task GetActiveProjectsAsync_ShouldReturnActiveProjects()
    {
        // Arrange
        var projects = new List<Project>
        {
            Project.Create("Active Project 1", ProjectType.Work),
            Project.Create("Active Project 2", ProjectType.Learning)
        };

        var expectedDtos = projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Type = p.Type
        });

        _projectRepositoryMock.Setup(r => r.GetActiveProjectsAsync())
            .ReturnsAsync(projects);
        _mapperMock.Setup(m => m.Map<IEnumerable<ProjectDto>>(projects))
            .Returns(expectedDtos);

        // Act
        var result = await _service.GetActiveProjectsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projects.Count, result.Count());
    }

    [Fact]
    public async Task AddSessionToProjectAsync_ShouldAddSessionToProject()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = Project.Create("Test Project", ProjectType.Work);
        var sessionDto = new CreateActivitySessionDto
        {
            Category = "Test Category",
            Description = "Test Session",
            Notes = "Test Notes",
            Progress = ActivityProgress.InProgress
        };

        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId))
            .ReturnsAsync(project);
        _sessionRepositoryMock.Setup(r => r.AddAsync(It.IsAny<WorkSession>()))
            .Returns(Task.CompletedTask);
        _projectRepositoryMock.Setup(r => r.GetWithSessionsAsync(projectId))
            .ReturnsAsync(project);

        // Act
        var result = await _service.AddSessionToProjectAsync(projectId, sessionDto);

        // Assert
        Assert.NotNull(result);
        _sessionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<WorkSession>()), Times.Once);
    }
} 