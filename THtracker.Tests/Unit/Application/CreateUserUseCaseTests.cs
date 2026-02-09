using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.DTOs;
using THtracker.Domain.Interfaces;

namespace THtracker.Tests.Unit.Application;

public class CreateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly CreateUserUseCase _useCase;

    public CreateUserUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _useCase = new CreateUserUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateUser_AndReturnUserDto()
    {
        // Arrange
        var createDto = new CreateUserDto("John Doe", "john@example.com");
        var expectedUser = new UserDto(Guid.NewGuid(), "John Doe", "john@example.com");

        _repositoryMock
            .Setup(x => x.CreateAsync(createDto))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _useCase.ExecuteAsync(createDto);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);
        _repositoryMock.Verify(x => x.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepository_WithCorrectDto()
    {
        // Arrange
        var createDto = new CreateUserDto("Jane Smith", "jane@example.com");
        var expectedUser = new UserDto(Guid.NewGuid(), "Jane Smith", "jane@example.com");

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(expectedUser);

        // Act
        await _useCase.ExecuteAsync(createDto);

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.Is<CreateUserDto>(dto => 
                dto.Name == createDto.Name && 
                dto.Email == createDto.Email)),
            Times.Once);
    }
}
