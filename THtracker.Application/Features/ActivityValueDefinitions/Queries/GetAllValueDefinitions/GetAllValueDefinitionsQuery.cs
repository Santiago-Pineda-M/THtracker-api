using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityValueDefinitions.Queries.GetAllValueDefinitions;

public sealed record GetAllValueDefinitionsQuery(
    Guid ActivityId,
    Guid UserId,
    int PageNumber = 1,
    int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PaginatedResponse<ActivityValueDefinitionResponse>>>;
