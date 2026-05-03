using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityValueDefinitions.Queries.GetAllValueDefinitions;

public sealed record GetAllValueDefinitionsQuery(Guid ActivityId, Guid UserId) : IRequest<Result<IEnumerable<ActivityValueDefinitionResponse>>>;
