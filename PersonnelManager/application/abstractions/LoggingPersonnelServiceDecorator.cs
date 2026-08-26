using PersonnelManager.Domain;

namespace PersonnelManager.Application.Abstractions;

/// <summary>
/// Logs around every IPersonnelService call. Replaces the two generic handler decorators from the
/// CQRS design — one decorator now covers all operations, because they live on one interface.
/// </summary>
public sealed class LoggingPersonnelServiceDecorator(IPersonnelService inner, IAppLogger logger)
    : IPersonnelService
{
    public Task<Result<PersonalDto>> CreateAsync(CreatePersonalRequest request, CancellationToken cancellationToken = default) =>
        LoggedAsync(nameof(CreateAsync), () => inner.CreateAsync(request, cancellationToken));

    public Task<Result<PersonalDto>> UpdateAsync(UpdatePersonalRequest request, CancellationToken cancellationToken = default) =>
        LoggedAsync(nameof(UpdateAsync), () => inner.UpdateAsync(request, cancellationToken));

    public Task<Result<Guid>> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        LoggedAsync(nameof(DeleteAsync), () => inner.DeleteAsync(id, cancellationToken));

    public Task<Result<PersonalDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        LoggedAsync(nameof(GetByIdAsync), () => inner.GetByIdAsync(id, cancellationToken));

    public Task<IReadOnlyList<PersonalDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        LoggedAsync(nameof(GetAllAsync), () => inner.GetAllAsync(cancellationToken));

    // Higher-order helper: takes the operation name and a delegate that performs the call, and wraps
    // logging + error handling around it once. Generic in T so it serves every method's return type.
    private async Task<T> LoggedAsync<T>(string operation, Func<Task<T>> action)
    {
        logger.Info($"{operation} starting.");
        try
        {
            var result = await action();
            logger.Info($"{operation} completed.");
            return result;
        }
        catch (Exception ex)
        {
            logger.Error($"{operation} failed: {ex.Message}");
            throw;
        }
    }
}
