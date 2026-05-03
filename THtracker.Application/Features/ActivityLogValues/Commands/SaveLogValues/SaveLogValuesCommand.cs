using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.ActivityLogValues.Commands.SaveLogValues;

public sealed record SaveLogValuesCommand(
    Guid ActivityLogId,
    IEnumerable<LogValueItem> Values,
    Guid UserId = default) : IRequest<Result<IEnumerable<LogValueResponse>>>;

public sealed record LogValueItem(Guid ValueDefinitionId, string Value);
