using Microsoft.EntityFrameworkCore;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Infrastructure.Persistence;

/// <summary>
/// The PostgreSQL-backed Personal store: the generic EF repository closed over Personal, tagged as
/// IPersonalRepository. Swapping this in for InMemoryPersonalRepository is a one-line DI change.
/// </summary>
public sealed class EfPersonalRepository(IDbContextFactory<PersonnelDbContext> contextFactory)
    : EfRepository<Personal>(contextFactory), IPersonalRepository;
