using FluentAssertions;
using FluentValidation;
using MediatR;
using THtracker.Application.Common.Behaviors;

namespace THtracker.Tests.Unit.Application.Common;

public sealed class ValidationBehaviorTests
{
    private sealed record SampleRequest(string Name) : IRequest<int>;

    private sealed class SampleValidator : AbstractValidator<SampleRequest>
    {
        public SampleValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidators()
    {
        var behaviors = Array.Empty<IValidator<SampleRequest>>();
        var sut = new ValidationBehavior<SampleRequest, int>(behaviors);
        var nextCalled = false;

        await sut.Handle(
            new SampleRequest("a"),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult(1);
            },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenInvalid()
    {
        var validators = new IValidator<SampleRequest>[] { new SampleValidator() };
        var sut = new ValidationBehavior<SampleRequest, int>(validators);

        var act = async () => await sut.Handle(
            new SampleRequest(""),
            _ => Task.FromResult(0),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValid()
    {
        var validators = new IValidator<SampleRequest>[] { new SampleValidator() };
        var sut = new ValidationBehavior<SampleRequest, int>(validators);
        var nextCalled = false;

        await sut.Handle(
            new SampleRequest("ok"),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult(1);
            },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
    }
}
