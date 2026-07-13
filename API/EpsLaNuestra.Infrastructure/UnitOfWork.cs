using EpsLaNuestra.Domain.Interfaces;
using EpsLaNuestra.Domain.Interfaces.Repositories;
using EpsLaNuestra.Infrastructure.Data;

namespace EpsLaNuestra.Infrastructure;

public class UnitOfWork(SqlServerDbContext context, IPatientSqlRepository patientSqlRepository) : IUnitOfWork
{
    public IPatientSqlRepository Copayments { get; } = patientSqlRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken) => await context.SaveChangesAsync(cancellationToken);
}