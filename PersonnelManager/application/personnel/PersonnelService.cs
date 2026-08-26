using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Application.Personnel;

/// <summary>
/// The concrete use cases. Depends on the repository and validator abstractions, injected via the
/// primary constructor. Each method is one operation that previously lived in its own handler.
/// </summary>
public sealed class PersonnelService(IPersonalRepository repository, IPersonalValidator validator)
    : IPersonnelService
{
    public async Task<Result<PersonalDto>> CreateAsync(
        CreatePersonalRequest request, CancellationToken cancellationToken = default)
    {
        var personal = new Personal
        {
            Name = request.Name,
            Surname = request.Surname,
            Address = request.Address,
            Phone = request.Phone,
            Status = request.Status,
        };

        var errors = validator.Validate(personal);
        if (errors.Count > 0)
            return Result<PersonalDto>.Failure(string.Join(" ", errors));

        await repository.AddAsync(personal, cancellationToken);
        return Result<PersonalDto>.Success(personal.ToDto());
    }

    public async Task<Result<PersonalDto>> UpdateAsync(
        UpdatePersonalRequest request, CancellationToken cancellationToken = default)
    {
        var personal = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (personal is null)
            return Result<PersonalDto>.Failure($"No person found with id {request.Id}.");

        personal.Name = request.Name;
        personal.Surname = request.Surname;
        personal.Address = request.Address;
        personal.Phone = request.Phone;
        personal.Status = request.Status;

        var updated = await repository.UpdateAsync(personal, cancellationToken);
        return updated
            ? Result<PersonalDto>.Success(personal.ToDto())
            : Result<PersonalDto>.Failure($"Could not update person {request.Id}.");
    }

    public async Task<Result<Guid>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        return deleted
            ? Result<Guid>.Success(id)
            : Result<Guid>.Failure($"No person found with id {id}.");
    }

    public async Task<Result<PersonalDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var personal = await repository.GetByIdAsync(id, cancellationToken);
        return personal switch
        {
            null => Result<PersonalDto>.Failure($"No person found with id {id}."),
            _ => Result<PersonalDto>.Success(personal.ToDto()),
        };
    }

    public async Task<IReadOnlyList<PersonalDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var people = await repository.GetAllAsync(cancellationToken);
        return [.. people.Select(person => person.ToDto())];
    }
}
