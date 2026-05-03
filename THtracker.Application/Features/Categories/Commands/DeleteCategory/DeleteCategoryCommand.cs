using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id, Guid UserId) : IRequest<Result<Unit>>;
