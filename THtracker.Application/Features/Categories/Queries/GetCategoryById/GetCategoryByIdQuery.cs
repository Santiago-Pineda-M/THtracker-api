using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id, Guid UserId) : IRequest<Result<CategoryResponse>>;
