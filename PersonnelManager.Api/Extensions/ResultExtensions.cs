using Microsoft.AspNetCore.Mvc;
using PersonnelManager.Application.Abstractions;

namespace PersonnelManager.Api.Extensions;

/// <summary>
/// Bridges the application layer's <see cref="Result{T}"/> to HTTP responses so controllers stay thin.
/// A failed result carrying a "No person found" message maps to 404; other failures map to 400.
/// This keeps the domain free of any HTTP knowledge (the Result type lives in the core library).
/// </summary>
public static class ResultExtensions
{
    /// <summary>Map a successful result to <paramref name="onSuccess"/>; a failure to 404/400 ProblemDetails.</summary>
    public static ActionResult<TResponse> ToActionResult<TValue, TResponse>(
        this Result<TValue> result,
        ControllerBase controller,
        Func<TValue, ActionResult<TResponse>> onSuccess) =>
        result.IsSuccess
            ? onSuccess(result.Value!)
            : controller.Problem(
                statusCode: IsNotFound(result.Error) ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                title: result.Error);

    /// <summary>Ok(value) on success; 404/400 ProblemDetails on failure.</summary>
    public static ActionResult<TValue> ToOk<TValue>(this Result<TValue> result, ControllerBase controller) =>
        result.ToActionResult<TValue, TValue>(controller, value => controller.Ok(value));

    private static bool IsNotFound(string? error) =>
        error is not null && error.Contains("No person found", StringComparison.OrdinalIgnoreCase);
}
