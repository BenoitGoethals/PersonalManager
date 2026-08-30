using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PersonnelManager.Api.Validation;

/// <summary>
/// A global MVC filter that runs any registered FluentValidation <see cref="IValidator{T}"/>
/// against each action argument before the action executes. On failure it short-circuits with a
/// 400 <c>ValidationProblemDetails</c> (RFC 7807) so every endpoint reports validation errors
/// consistently without repeating the plumbing.
/// </summary>
public sealed class FluentValidationFilter(IServiceProvider services) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new ModelStateDictionary();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument));
            if (result.IsValid)
                continue;

            foreach (var failure in result.Errors)
                errors.AddModelError(failure.PropertyName, failure.ErrorMessage);
        }

        if (!errors.IsValid)
        {
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors)
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
            });
            return;
        }

        await next();
    }
}
