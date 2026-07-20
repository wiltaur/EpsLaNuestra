using EpsLaNuestra.Domain.Entities;
using EpsLaNuestra.Infrastructure.Data;
using EpsLaNuestra.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EpsLaNuestra.Test.Repositories
{
    public class PatientSqlRepositoryTests
    {
        private SqlServerDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<SqlServerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new SqlServerDbContext(options);
        }

        [Fact]
        public async Task SaveTransactionAsync_ShouldInsert_WhenPatientDoesNotExist()
        {
            var context = GetInMemoryContext();
            var repository = new PatientSqlRepository(context);

            var copayment = new Copayment { PatientDocument = "123", CopaymentValue = 500 };

            await repository.SaveTransactionAsync(copayment, CancellationToken.None);
            await context.SaveChangesAsync();

            var saved = await context.Copayments.FindAsync("123");
            Assert.NotNull(saved);
            Assert.Equal(500, saved.CopaymentValue);
        }

        [Fact]
        public async Task SaveTransactionAsync_ShouldUpdate_WhenPatientExists()
        {
            var context = GetInMemoryContext();
            context.Copayments.Add(new Copayment { PatientDocument = "123", CopaymentValue = 200 });
            await context.SaveChangesAsync();

            var repository = new PatientSqlRepository(context);
            var copayment = new Copayment { PatientDocument = "123", CopaymentValue = 800 };

            await repository.SaveTransactionAsync(copayment, CancellationToken.None);
            await context.SaveChangesAsync();

            var updated = await context.Copayments.FindAsync("123");
            Assert.Equal(800, updated!.CopaymentValue);
        }
    }
}