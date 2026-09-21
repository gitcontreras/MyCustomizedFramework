using Microsoft.AspNetCore.Mvc;
using MyCustomizedFramework.Domain.Common;

namespace MyCustomizedFramework.Api.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<TValue, TResponse>(
        this Result<TValue> result,
        Func<TValue, TResponse> map)
    {
        return result.IsSuccess
            ? new OkObjectResult(map(result.Value))
            : result.ToProblem();
    }

    public static ObjectResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Only a failed result can be converted to a problem response.");
        }

        var statusCode = result.Error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Title = result.Error.Code,
            Detail = result.Error.Message,
            Status = statusCode
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
