using FluentValidation;
using THtracker.Application.DTOs.ActivityLogs;

namespace THtracker.Application.Validators.ActivityLogs;

public class GetActiveActivityLogsRequestValidator : AbstractValidator<GetActiveActivityLogsRequest>
{
    public GetActiveActivityLogsRequestValidator()
    {
    }
}
