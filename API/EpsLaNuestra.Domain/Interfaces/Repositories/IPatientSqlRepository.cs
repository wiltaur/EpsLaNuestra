using EpsLaNuestra.Domain.Entities;

namespace EpsLaNuestra.Domain.Interfaces.Repositories;

public interface IPatientSqlRepository
{
    Task SaveTransactionAsync(Copayment copayment, CancellationToken cancellationToken);
}