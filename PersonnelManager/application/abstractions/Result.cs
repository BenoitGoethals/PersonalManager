namespace PersonnelManager.Application.Abstractions;

/// <summary>
/// A tiny functional Result type so handlers can report success/failure without throwing
/// exceptions for expected outcomes (e.g. "not found"). C# has no discriminated unions yet,
/// so we model it as a `readonly record struct` — a lightweight, allocation-free value type
/// with built-in equality.
/// </summary>
public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);

    /// <summary>
    /// Pattern-friendly matcher: forces the caller to handle both branches.
    /// Demonstrates passing behaviour as delegates (functional style).
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<string, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}
