using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name, 
    string? Description, 
    string Color,
    Guid UserId = default) : IRequest<Result<CategoryResponse>>;
