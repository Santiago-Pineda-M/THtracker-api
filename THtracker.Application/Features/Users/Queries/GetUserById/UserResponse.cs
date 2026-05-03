namespace THtracker.Application.Features.Users.Queries.GetUserById;

public sealed record UserResponse(Guid Id, string Name, string Email);
