using FluentAssertions;
using FluentValidation.TestHelper;
using THtracker.Application.DTOs.Users;
using THtracker.Application.Validators.Users;
using Xunit;

namespace THtracker.Tests.Unit.Application.Users;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _validator;

    public UpdateUserRequestValidatorTests()
    {
        _validator = new UpdateUserRequestValidator();
    }

    [Fact]
    public void Should_HaveError_When_NameIsEmpty()
    {
        var request = new UpdateUserRequest("", "test@example.com");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Name is required");
    }

    [Fact]
    public void Should_HaveError_When_NameExceedsMaxLength()
    {
        var longName = new string('a', 101);
        var request = new UpdateUserRequest(longName, "test@example.com");

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Fact]
    public void Should_HaveError_When_EmailIsEmpty()
    {
        var request = new UpdateUserRequest("John Doe", "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required");
    }

    [Fact]
    public void Should_HaveError_When_EmailIsInvalid()
    {
        var request = new UpdateUserRequest("John Doe", "not-an-email");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Should_HaveError_When_EmailExceedsMaxLength()
    {
        var longEmail = new string('a', 140) + "@example.com";
        var request = new UpdateUserRequest("John Doe", longEmail);

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 150 characters");
    }

    [Fact]
    public void Should_NotHaveError_When_RequestIsValid()
    {
        var request = new UpdateUserRequest("John Doe", "john@example.com");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
