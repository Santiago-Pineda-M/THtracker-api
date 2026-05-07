using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Categories.Queries.GetAllCategories;

public sealed class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<PaginatedResponse<CategoryResponse>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<PaginatedResponse<CategoryResponse>>> Handle(
        GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var page = await _categoryRepository.GetPageByUserAsync(
            request.UserId,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = page.Items
            .Select(c => new CategoryResponse(
                c.Id,
                c.UserId,
                c.Name,
                c.Description,
                c.Color))
            .ToList();

        return Result.Success(new PaginatedResponse<CategoryResponse>(
            items,
            page.TotalCount,
            pageNumber,
            pageSize));
    }
}
