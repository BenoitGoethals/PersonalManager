using PersonnelManager.Application.Abstractions;
using PersonnelManager.Application.Personnel;
using PersonnelManager.Domain;
using PersonnelManager.Infrastructure;
using PersonnelManager.Tests.Fakes;

namespace PersonnelManager.Tests;

public class PersonnelServiceTests
{
    private static PersonnelService NewService(IPersonalRepository repository) =>
        new(repository, new PersonalValidator());

    // --- Create ---

    [Fact]
    public async Task Create_WithValidData_PersistsAndReturnsDto()
    {
        var repository = new InMemoryPersonalRepository();
        var service = NewService(repository);

        var result = await service.CreateAsync(new CreatePersonalRequest("Ada", "Lovelace", "London", "+44 100"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Ada", result.Value!.Name);
        Assert.NotNull(await repository.GetByIdAsync(result.Value.Id));
    }

    [Fact]
    public async Task Create_WithBlankNameAndSurname_FailsValidation()
    {
        var repository = new InMemoryPersonalRepository();
        var service = NewService(repository);

        var result = await service.CreateAsync(new CreatePersonalRequest("  ", null, "London", "+44 100"));

        Assert.False(result.IsSuccess);
        Assert.Empty(await repository.GetAllAsync());
    }

    [Fact]
    public async Task Create_TrimsWhitespace()
    {
        var service = NewService(new InMemoryPersonalRepository());

        var result = await service.CreateAsync(new CreatePersonalRequest("  Ada  ", "  Lovelace  ", null, null));

        Assert.Equal("Ada", result.Value!.Name);
        Assert.Equal("Lovelace", result.Value.Surname);
    }

    [Fact]
    public async Task Create_WithExplicitStatus_PersistsIt()
    {
        var service = NewService(new InMemoryPersonalRepository());

        var result = await service.CreateAsync(
            new CreatePersonalRequest("Alan", "Turing", null, null, EmploymentStatus.OnLeave));

        Assert.Equal(EmploymentStatus.OnLeave, result.Value!.Status);
    }

    // --- Update ---

    [Fact]
    public async Task Update_ExistingPerson_ChangesFields()
    {
        var repository = new InMemoryPersonalRepository();
        var person = new Personal { Name = "Alan", Surname = "Turing" };
        await repository.AddAsync(person);
        var service = NewService(repository);

        var result = await service.UpdateAsync(
            new UpdatePersonalRequest(person.Id, "Alan", "Turing", "Manchester", "+44 200", EmploymentStatus.Terminated));

        Assert.True(result.IsSuccess);
        Assert.Equal("Manchester", result.Value!.Address);
        Assert.Equal(EmploymentStatus.Terminated, (await repository.GetByIdAsync(person.Id))!.Status);
    }

    [Fact]
    public async Task Update_MissingPerson_ReturnsFailure()
    {
        var service = NewService(new InMemoryPersonalRepository());

        var result = await service.UpdateAsync(new UpdatePersonalRequest(Guid.NewGuid(), "X", null, null, null));

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Update_WhenPersistenceFails_ReturnsFailure()
    {
        // Entity exists, but the store reports the update didn't persist.
        var existing = new Personal { Name = "Grace", Surname = "Hopper" };
        var service = NewService(new StubPersonalRepository(existing, updateSucceeds: false));

        var result = await service.UpdateAsync(
            new UpdatePersonalRequest(existing.Id, "Grace", "Hopper", "NYC", null));

        Assert.False(result.IsSuccess);
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_ExistingPerson_RemovesAndReturnsId()
    {
        var repository = new InMemoryPersonalRepository();
        var person = new Personal { Name = "Ada", Surname = "Lovelace" };
        await repository.AddAsync(person);
        var service = NewService(repository);

        var result = await service.DeleteAsync(person.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(person.Id, result.Value);
        Assert.Null(await repository.GetByIdAsync(person.Id));
    }

    [Fact]
    public async Task Delete_MissingPerson_ReturnsFailure()
    {
        var service = NewService(new InMemoryPersonalRepository());

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
    }

    // --- Queries ---

    [Fact]
    public async Task GetById_Existing_ReturnsDto()
    {
        var repository = new InMemoryPersonalRepository();
        var person = new Personal { Name = "Ada", Surname = "Lovelace" };
        await repository.AddAsync(person);
        var service = NewService(repository);

        var result = await service.GetByIdAsync(person.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Lovelace", result.Value!.Surname);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsFailure()
    {
        var service = NewService(new InMemoryPersonalRepository());

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmpty()
    {
        var service = NewService(new InMemoryPersonalRepository());

        Assert.Empty(await service.GetAllAsync());
    }

    [Fact]
    public async Task GetAll_WithRecords_ReturnsAllAsDtos()
    {
        var repository = new InMemoryPersonalRepository();
        await repository.AddAsync(new Personal { Name = "Ada", Surname = "Lovelace" });
        await repository.AddAsync(new Personal { Name = "Alan", Surname = "Turing" });
        var service = NewService(repository);

        var all = await service.GetAllAsync();

        Assert.Equal(2, all.Count);
        Assert.Contains(all, dto => dto.Surname == "Turing");
    }
}
