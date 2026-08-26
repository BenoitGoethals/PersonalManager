using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Infrastructure;

/// <summary>
/// The Personal store. It inherits all five CRUD methods from the generic base and simply
/// tags itself as IPersonalRepository. Adding a store for a new entity later is the same
/// one-liner: `class InMemoryOrderRepository : InMemoryRepository&lt;Order&gt;, IOrderRepository`.
/// </summary>
public sealed class InMemoryPersonalRepository : InMemoryRepository<Personal>, IPersonalRepository
{
}
