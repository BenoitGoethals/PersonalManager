namespace PersonnelManager.Domain;

/// <summary>
/// The one thing every stored entity must expose: a stable identity.
/// A generic repository will CONSTRAIN its type parameter to this interface
/// (`where TEntity : IEntity`) so it can read `.Id` without knowing the concrete type.
/// </summary>
public interface IEntity
{
    Guid Id { get; }
}
