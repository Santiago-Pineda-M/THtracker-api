using FluentAssertions;
using FluentValidation.TestHelper;
using THtracker.Application.DTOs.Users;
using THtracker.Application.Validators.Users;
using Xunit;

namespace THtracker.Tests.Unit.Application.Users;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator;

    public CreateUserRequestValidatorTests()
    {
        _validator = new CreateUserRequestValidator();
    }

    [Fact]
    public void Should_HaveError_When_NameIsEmpty()
    {
        var request = new CreateUserRequest("", "test@example.com");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Name is required");
    }

    [Fact]
    public void Should_HaveError_When_NameExceedsMaxLength()
    {
        var longName = new string('a', 101);
        var request = new CreateUserRequest(longName, "test@example.com");

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Fact]
    public void Should_HaveError_When_EmailIsEmpty()
    {
        var request = new CreateUserRequest("John Doe", "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required");
    }

    [Fact]
    public void Should_HaveError_When_EmailIsInvalid()
    {
        var request = new CreateUserRequest("John Doe", "not-an-email");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Should_HaveError_When_EmailExceedsMaxLength()
    {
        var longEmail = new string('a', 140) + "@example.com";
        var request = new CreateUserRequest("John Doe", longEmail);

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 150 characters");
    }

    [Fact]
    public void Should_NotHaveError_When_RequestIsValid()
    {
        var request = new CreateUserRequest("John Doe", "john@example.com");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
