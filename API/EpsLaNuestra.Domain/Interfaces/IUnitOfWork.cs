using EpsLaNuestra.Domain.Interfaces.Repositories;

namespace EpsLaNuestra.Domain.Interfaces;

public interface IUnitOfWork
{
    IPatientSqlRepository Copayments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}