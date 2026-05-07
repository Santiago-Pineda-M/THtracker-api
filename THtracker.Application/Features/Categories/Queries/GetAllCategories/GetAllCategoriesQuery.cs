using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Categories.Queries.GetAllCategories;

public sealed record GetAllCategoriesQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PaginatedResponse<CategoryResponse>>>;
