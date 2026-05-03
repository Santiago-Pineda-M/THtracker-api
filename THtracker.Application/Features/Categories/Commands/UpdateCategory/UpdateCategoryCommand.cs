using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    string Color,
    Guid UserId = default) : IRequest<Result<CategoryResponse>>;
