using FluentAssertions;
using Moq;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Interfaces;

namespace THtracker.Tests.Unit.Application.Users
{
    [Trait("Category", "Unit")]
    [Trait("Layer", "Application")]
    public class DeleteUserUseCaseTests
    {
        private readonly Mock<IUserRepository> _repositoryMock;
        private readonly DeleteUserUseCase _useCase;

        public DeleteUserUseCaseTests()
        {
            _repositoryMock = new Mock<IUserRepository>();
            _useCase = new DeleteUserUseCase(_repositoryMock.Object);
        }

        [Fact]
        public async Task ExecuteAsyncShouldReturnTrueWhenUserIsDeleted()
        {
            Guid userId = Guid.NewGuid();
            _ = _repositoryMock.Setup(x => x.DeleteAsync(userId)).ReturnsAsync(true);

            bool result = await _useCase.ExecuteAsync(userId);

            _ = result.Should().BeTrue();
            _repositoryMock.Verify(x => x.DeleteAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsyncShouldReturnFalseWhenUserDoesNotExist()
        {
            Guid userId = Guid.NewGuid();
            _ = _repositoryMock.Setup(x => x.DeleteAsync(userId)).ReturnsAsync(false);

            bool result = await _useCase.ExecuteAsync(userId);

            _ = result.Should().BeFalse();
            _repositoryMock.Verify(x => x.DeleteAsync(userId), Times.Once);
        }
    }
}
