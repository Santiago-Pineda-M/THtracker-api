using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Categories.Queries.GetAllCategories;

public sealed record GetAllCategoriesQuery(Guid UserId) : IRequest<IEnumerable<CategoryResponse>>;
