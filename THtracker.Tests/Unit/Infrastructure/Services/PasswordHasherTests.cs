using FluentAssertions;
using THtracker.Infrastructure.Services;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void Hash_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hashedPassword = _passwordHasher.Hash(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().Contain("-"); // Hash format: hash-salt
        var parts = hashedPassword.Split('-');
        parts.Should().HaveCount(2);
        parts[0].Should().NotBeNullOrEmpty(); // Hash part
        parts[1].Should().NotBeNullOrEmpty(); // Salt part
    }

    [Fact]
    public void Hash_ShouldReturnDifferentHashes_ForSamePassword()
    {
        // Arrange
        var password = "SamePassword123";

        // Act
        var hash1 = _passwordHasher.Hash(password);
        var hash2 = _passwordHasher.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2); // Different salts should produce different hashes
    }

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordMatches()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var hashedPassword = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatch()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hashedPassword = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("VeryLongPasswordWithManyCharacters123456789!@#$%^&*()")]
    [InlineData("Password with spaces")]
    [InlineData("Contraseña123")]
    public void Hash_And_Verify_ShouldWorkCorrectly_ForVariousPasswords(string password)
    {
        // Act
        var hashedPassword = _passwordHasher.Hash(password);
        var verifyResult = _passwordHasher.Verify(password, hashedPassword);

        // Assert
        verifyResult.Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldBeCaseSensitive()
    {
        // Arrange
        var password = "CaseSensitive123";
        var hashedPassword = _passwordHasher.Hash(password);

        // Act
        var resultLowerCase = _passwordHasher.Verify("casesensitive123", hashedPassword);
        var resultUpperCase = _passwordHasher.Verify("CASESENSITIVE123", hashedPassword);
        var resultCorrect = _passwordHasher.Verify(password, hashedPassword);

        // Assert
        resultLowerCase.Should().BeFalse();
        resultUpperCase.Should().BeFalse();
        resultCorrect.Should().BeTrue();
    }
}
