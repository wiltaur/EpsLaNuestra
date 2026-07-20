using EpsLaNuestra.Domain.Entities;
using EpsLaNuestra.Domain.Interfaces.Repositories;
using EpsLaNuestra.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
        {
            var existing = await _context.Copayments
                .FirstOrDefaultAsync(c => c.PatientDocument == copayment.PatientDocument, cancellationToken);

            if (existing is null)
                await _context.Copayments.AddAsync(copayment, cancellationToken);
            else
                existing.CopaymentValue = copayment.CopaymentValue;
        }
    }
}