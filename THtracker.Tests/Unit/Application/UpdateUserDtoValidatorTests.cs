using FluentAssertions;
using FluentValidation.TestHelper;
using THtracker.Application.Validators;
using THtracker.Domain.DTOs;

namespace THtracker.Tests.Unit.Application;

public class UpdateUserDtoValidatorTests
{
    private readonly UpdateUserDtoValidator _validator;

    public UpdateUserDtoValidatorTests()
    {
        _validator = new UpdateUserDtoValidator();
    }

    [Fact]
    public void Should_HaveError_When_NameIsEmpty()
    {
        // Arrange
        var dto = new UpdateUserDto("", "test@example.com");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Should_HaveError_When_NameExceedsMaxLength()
    {
        // Arrange
        var longName = new string('a', 101);
        var dto = new UpdateUserDto(longName, "test@example.com");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Fact]
    public void Should_HaveError_When_EmailIsEmpty()
    {
        // Arrange
        var dto = new UpdateUserDto("John Doe", "");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Fact]
    public void Should_HaveError_When_EmailIsInvalid()
    {
        // Arrange
        var dto = new UpdateUserDto("John Doe", "not-an-email");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Should_HaveError_When_EmailExceedsMaxLength()
    {
        // Arrange
        var longEmail = new string('a', 140) + "@example.com";
        var dto = new UpdateUserDto("John Doe", longEmail);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 150 characters");
    }

    [Fact]
    public void Should_NotHaveError_When_DtoIsValid()
    {
        // Arrange
        var dto = new UpdateUserDto("John Doe", "john@example.com");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
