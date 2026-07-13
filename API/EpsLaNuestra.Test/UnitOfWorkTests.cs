using EpsLaNuestra.Domain.Interfaces.Repositories;
using EpsLaNuestra.Infrastructure;
using EpsLaNuestra.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EpsLaNuestra.Test;

public class UnitOfWorkTests
{
    private Mock<SqlServerDbContext> _mockContext;
    private Mock<IPatientSqlRepository> _mockRepo;
    private UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<SqlServerDbContext>()
            .Options;
        _mockContext = new Mock<SqlServerDbContext>(options);
        _mockRepo = new Mock<IPatientSqlRepository>();
        _unitOfWork = new UnitOfWork(_mockContext.Object, _mockRepo.Object);
    }

    [Fact]
    public void Copayments_ShouldReturnInjectedRepository()
    {
        Assert.Equal(_unitOfWork.Copayments, _mockRepo.Object);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldCallContextSaveChangesAsync_AndReturnResult()
    {
        var cancellationToken = new CancellationToken();
        _mockContext.Setup(c => c.SaveChangesAsync(cancellationToken)).ReturnsAsync(42);

        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

        Assert.Equal(42, result);
        _mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
    }
}