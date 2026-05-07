using THtracker.Domain.Common;
using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<PagedList<Category>> GetPageByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Category>> GetAllByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
