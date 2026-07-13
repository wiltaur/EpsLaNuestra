using EpsLaNuestra.Domain.Entities;
using EpsLaNuestra.Domain.Interfaces.Repositories;
using EpsLaNuestra.Infrastructure.Data;

namespace EpsLaNuestra.Infrastructure.Repositories
{
    public class PatientSqlRepository : IPatientSqlRepository
    {
        private readonly SqlServerDbContext _context;

        public PatientSqlRepository(SqlServerDbContext context)
        {
            _context = context;
        }

        public async Task SaveTransactionAsync(Copayment copayment, CancellationToken cancellationToken)
        => await _context.Copayments.AddAsync(copayment, cancellationToken);
    }
}