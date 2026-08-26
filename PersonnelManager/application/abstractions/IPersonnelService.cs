using PersonnelManager.Domain;

namespace PersonnelManager.Application.Abstractions;

// Plain input DTOs. These used to be ICommand<...> records; without CQRS they're just
// parameter objects that keep the service methods tidy (no long argument lists).
public record CreatePersonalRequest(
    string? Name, string? Surname, string? Address, string? Phone,
    EmploymentStatus Status = EmploymentStatus.Active);

public record UpdatePersonalRequest(
    Guid Id, string? Name, string? Surname, string? Address, string? Phone,
    EmploymentStatus Status = EmploymentStatus.Active);

/// <summary>
/// The single application service for personnel. It replaces the five separate CQRS handlers
/// with one cohesive use-case surface — simpler for an app of this size, and still an abstraction
/// the presentation layer depends on (Dependency Inversion is unchanged).
/// </summary>
public interface IPersonnelService
{
    Task<Result<PersonalDto>> CreateAsync(CreatePersonalRequest request, CancellationToken cancellationToken = default);
    Task<Result<PersonalDto>> UpdateAsync(UpdatePersonalRequest request, CancellationToken cancellationToken = default);
    Task<Result<Guid>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PersonalDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PersonalDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
