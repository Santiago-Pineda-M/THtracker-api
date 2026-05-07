namespace THtracker.Application.Interfaces;

public interface ICurrentUserService
{
    Guid GetUserId();

    Guid GetSessionId();
}
