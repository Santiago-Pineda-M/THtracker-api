using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Repositories;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class ActivityValueDefinitionRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ActivityValueDefinitionRepository _repository;

    public ActivityValueDefinitionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new ActivityValueDefinitionRepository(_context);
    }

    [Fact]
    public async Task GetAllByActivityAsync_ShouldReturnValueDefinitions_ForSpecificActivity()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var otherActivityId = Guid.NewGuid();

        var def1 = new ActivityValueDefinition(activityId, "Distance", "number", true, "km");
        var def2 = new ActivityValueDefinition(activityId, "Duration", "time", true, "minutes");
        var defOther = new ActivityValueDefinition(otherActivityId, "Other", "text");

        _context.ActivityValueDefinitions.AddRange(def1, def2, defOther);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByActivityAsync(activityId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Id == def1.Id);
        result.Should().Contain(d => d.Id == def2.Id);
        result.Should().NotContain(d => d.Id == defOther.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnValueDefinition_WhenExists()
    {
        // Arrange
        var definition = new ActivityValueDefinition(
            Guid.NewGuid(),
            "Speed",
            "number",
            true,
            "km/h",
            "0",
            "200"
        );
        _context.ActivityValueDefinitions.Add(definition);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(definition.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(definition.Id);
        result.Name.Should().Be("Speed");
        result.ValueType.Should().Be("number");
        result.Unit.Should().Be("km/h");
        result.MinValue.Should().Be("0");
        result.MaxValue.Should().Be("200");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddValueDefinition()
    {
        // Arrange
        var definition = new ActivityValueDefinition(
            Guid.NewGuid(),
            "Heart Rate",
            "number",
            false,
            "bpm"
        );

        // Act
        await _repository.AddAsync(definition);
        await _context.SaveChangesAsync();

        // Assert
        var definitionInDb = await _context.ActivityValueDefinitions.FindAsync(definition.Id);
        definitionInDb.Should().NotBeNull();
        definitionInDb!.Name.Should().Be("Heart Rate");
        definitionInDb.ValueType.Should().Be("number");
        definitionInDb.Unit.Should().Be("bpm");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateValueDefinition()
    {
        // Arrange
        var definition = new ActivityValueDefinition(
            Guid.NewGuid(),
            "Original Name",
            "text"
        );
        _context.ActivityValueDefinitions.Add(definition);
        await _context.SaveChangesAsync();

        // Act
        definition.Update("Updated Name", "number", true, "meters", "0", "1000");
        await _repository.UpdateAsync(definition);
        await _context.SaveChangesAsync();

        // Assert
        var definitionInDb = await _context.ActivityValueDefinitions.FindAsync(definition.Id);
        definitionInDb!.Name.Should().Be("Updated Name");
        definitionInDb.ValueType.Should().Be("number");
        definitionInDb.IsRequired.Should().BeTrue();
        definitionInDb.Unit.Should().Be("meters");
        definitionInDb.MinValue.Should().Be("0");
        definitionInDb.MaxValue.Should().Be("1000");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteValueDefinition_WhenExists()
    {
        // Arrange
        var definition = new ActivityValueDefinition(Guid.NewGuid(), "To Delete", "text");
        _context.ActivityValueDefinitions.Add(definition);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(definition.Id);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var definitionInDb = await _context.ActivityValueDefinitions.FindAsync(definition.Id);
        definitionInDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
