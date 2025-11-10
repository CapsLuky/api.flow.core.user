using Core.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToIResult<T>(this Result<T> result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            // Retorna { value: ..., isSuccess: true }
            return TypedResults.Ok(new
            {
                value     = result.Value,
                isSuccess = true
            });
        }

        var problem = new ProblemDetails
        {
            Type     = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title    = "Request Error",
            Status   = StatusCodes.Status400BadRequest,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["errors"]  = result.Errors;

        return TypedResults.BadRequest(problem);
    }
}
