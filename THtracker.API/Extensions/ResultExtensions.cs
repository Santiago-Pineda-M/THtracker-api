using MediatR;
using Microsoft.AspNetCore.Mvc;
using THtracker.Domain.Common;

namespace THtracker.API.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return result is Result<object> resultWithValue 
                ? new OkObjectResult(resultWithValue.Value) 
                : new NoContentResult();
        }

        return MapErrorToActionResult(result.Error);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            if (typeof(T) == typeof(Unit))
                return new NoContentResult();
            return new OkObjectResult(result.Value!);
        }

        return MapErrorToActionResult(result.Error);
    }

    private static IActionResult MapErrorToActionResult(Error error)
    {
        var statusCode = error.Code switch
        {
            "Validation" => StatusCodes.Status400BadRequest,
            "NotFound" => StatusCodes.Status404NotFound,
            "Unauthorized" => StatusCodes.Status401Unauthorized,
            "Forbidden" => StatusCodes.Status403Forbidden,
            "Conflict" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return new ObjectResult(new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Description,
            Type = $"https://thtracker.com/errors/{error.Code.ToLower()}"
        })
        {
            StatusCode = statusCode
        };
    }
}