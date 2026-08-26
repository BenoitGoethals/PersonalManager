using PersonnelManager.Domain;
using PersonnelManager.Infrastructure;

namespace PersonnelManager.Tests;

/// <summary>
/// Proves the payoff of the generic refactor: InMemoryRepository&lt;TEntity&gt; wasn't written for
/// Personal, so it works for a completely unrelated entity with zero extra code. A local record
/// that implements IEntity is enough to close the type parameter.
/// </summary>
public class GenericRepositoryTests
{
    // A throwaway entity that exists only for this test. Positional record → `Id { get; init; }`
    // which satisfies IEntity's `Guid Id { get; }`.
    private sealed record Widget(Guid Id, string Label) : IEntity;

    [Fact]
    public async Task GenericRepository_DoesCrud_ForAnyEntityType()
    {
        // Same base class the Personal repository inherits — closed over Widget this time.
        var repository = new InMemoryRepository<Widget>();
        var widget = new Widget(Guid.NewGuid(), "gizmo");

        await repository.AddAsync(widget);
        Assert.Equal(widget, await repository.GetByIdAsync(widget.Id));
        Assert.Single(await repository.GetAllAsync());

        Assert.True(await repository.UpdateAsync(widget with { Label = "gadget" }));
        Assert.Equal("gadget", (await repository.GetByIdAsync(widget.Id))!.Label);

        Assert.True(await repository.DeleteAsync(widget.Id));
        Assert.Null(await repository.GetByIdAsync(widget.Id));
    }

    [Fact]
    public async Task GenericRepository_Update_MissingEntity_ReturnsFalse()
    {
        var repository = new InMemoryRepository<Widget>();

        var updated = await repository.UpdateAsync(new Widget(Guid.NewGuid(), "nope"));

        Assert.False(updated);
    }
}
