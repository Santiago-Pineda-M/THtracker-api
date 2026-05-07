using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<Category>> GetPageByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Categories.AsNoTracking().Where(c => c.UserId == userId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedList<Category>(items, total);
    }

    public async Task<IEnumerable<Category>> GetAllByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Categories.Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Categories.Update(category);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
        if (category == null)
            return false;
        _context.Categories.Remove(category);
        return true;
    }
}
