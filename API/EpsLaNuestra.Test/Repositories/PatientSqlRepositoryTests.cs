
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using EpsLaNuestra.Infrastructure.Repositories;
using EpsLaNuestra.Infrastructure.Data;
using EpsLaNuestra.Domain.Entities;

namespace EpsLaNuestra.Test.Repositories
{
    public class PatientSqlRepositoryTests
    {
        [Fact]
        public async Task SaveTransactionAsync_Calls_AddAsync_With_Correct_Parameters()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Copayment>>();
            mockSet
                .Setup(s => s.AddAsync(It.IsAny<Copayment>(), It.IsAny<CancellationToken>()))
                .Returns((Copayment cp, CancellationToken ct) =>
                    new ValueTask<EntityEntry<Copayment>>((EntityEntry<Copayment>)null));

            var options = new DbContextOptions<SqlServerDbContext>();
            var mockContext = new Mock<SqlServerDbContext>(options);
            mockContext.SetupGet(c => c.Copayments).Returns(mockSet.Object);

            var repository = new PatientSqlRepository(mockContext.Object);

            var copayment = new Copayment { PatientDocument = "123", CopaymentValue = 50 };
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            // Act
            await repository.SaveTransactionAsync(copayment, token);

            // Assert
            mockSet.Verify(
                s => s.AddAsync(
                    It.Is<Copayment>(p => ReferenceEquals(p, copayment)),
                    It.Is<CancellationToken>(t => t == token)
                ),
                Times.Once);
        }

        [Fact]
        public async Task SaveTransactionAsync_Does_Not_Call_SaveChangesAsync()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Copayment>>();
            mockSet
                .Setup(s => s.AddAsync(It.IsAny<Copayment>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<EntityEntry<Copayment>>((EntityEntry<Copayment>)null));

            var options = new DbContextOptions<SqlServerDbContext>();
            var mockContext = new Mock<SqlServerDbContext>(options);
            mockContext.SetupGet(c => c.Copayments).Returns(mockSet.Object);

            var repository = new PatientSqlRepository(mockContext.Object);

            var copayment = new Copayment { PatientDocument = "456", CopaymentValue = 75 };
            var token = CancellationToken.None;

            // Act
            await repository.SaveTransactionAsync(copayment, token);

            // Assert
            mockContext.Verify(
                c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}