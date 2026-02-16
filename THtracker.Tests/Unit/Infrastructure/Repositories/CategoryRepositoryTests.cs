using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using Xunit;

namespace THtracker.Tests.Unit.Infrastructure.Repositories;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
public class CategoryRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new CategoryRepository(_context);
    }

    [Fact]
    public async Task GetAllByUserAsync_ShouldReturnCategories_ForSpecificUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var category1 = new Category(userId, "Category 1");
        var category2 = new Category(userId, "Category 2");
        var categoryOther = new Category(otherUserId, "Other Category");

        _context.Categories.AddRange(category1, category2, categoryOther);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByUserAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == category1.Id);
        result.Should().Contain(c => c.Id == category2.Id);
        result.Should().NotContain(c => c.Id == categoryOther.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var category = new Category(Guid.NewGuid(), "Test Category");
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be("Test Category");
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
    public async Task AddAsync_ShouldAddCategory()
    {
        // Arrange
        var category = new Category(Guid.NewGuid(), "New Category");

        // Act
        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        // Assert
        var categoryInDb = await _context.Categories.FindAsync(category.Id);
        categoryInDb.Should().NotBeNull();
        categoryInDb!.Name.Should().Be("New Category");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory()
    {
        // Arrange
        var category = new Category(Guid.NewGuid(), "Original Name");
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        category.UpdateName("Updated Name");
        await _repository.UpdateAsync(category);
        await _context.SaveChangesAsync();

        // Assert
        var categoryInDb = await _context.Categories.FindAsync(category.Id);
        categoryInDb!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategory_WhenExists()
    {
        // Arrange
        var category = new Category(Guid.NewGuid(), "To Delete");
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(category.Id);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var categoryInDb = await _context.Categories.FindAsync(category.Id);
        categoryInDb.Should().BeNull();
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
