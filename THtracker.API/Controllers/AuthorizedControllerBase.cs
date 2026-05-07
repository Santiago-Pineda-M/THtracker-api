using Microsoft.AspNetCore.Mvc;
using THtracker.Application.Interfaces;

namespace THtracker.API.Controllers;

public abstract class AuthorizedControllerBase : ControllerBase
{
    private readonly ICurrentUserService _currentUser;

    protected AuthorizedControllerBase(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    protected Guid GetUserId() => _currentUser.GetUserId();

    protected Guid GetSessionId() => _currentUser.GetSessionId();
}
