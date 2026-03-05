using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

public class ValidationFilterAttribute(ILogger<ValidationFilterAttribute> logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var validationErrors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => new ValidationError
            {
                Field = x.Key,
                Messages = x.Value?.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList() ?? new List<string>()
            })
            .ToList();

        logger.LogWarning("Validation failed for request to {ActionName}. Errors: {@ValidationErrors}",
            context.ActionDescriptor.DisplayName,
            validationErrors);

        var problemDetails = new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred",
            Detail = "Please refer to the errors property for additional details",
            Instance = context.HttpContext.Request.Path
        };

        foreach (var error in validationErrors)
        {
            problemDetails.Errors.Add(error.Field, error.Messages.ToArray());
        }

        var validationResult = new BadRequestObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        context.Result = validationResult;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}

public class ValidationError
{
    public string Field { get; init; } = string.Empty;
    public List<string> Messages { get; init; } = [];
}