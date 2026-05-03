using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityValueDefinitions.Queries.GetValueDefinitionById;

public sealed record GetValueDefinitionByIdQuery(Guid ActivityId, Guid DefinitionId, Guid UserId) : IRequest<Result<ActivityValueDefinitionResponse>>;
