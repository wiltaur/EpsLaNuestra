using Moq;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using Polly;
using EpsLaNuestra.Application.Features.Patients.Commands;
using EpsLaNuestra.Application.DTOs;
using EpsLaNuestra.Domain.Interfaces;
using EpsLaNuestra.Domain.Interfaces.Repositories;
using EpsLaNuestra.Domain.Entities;

namespace EpsLaNuestra.Test.Commands;

public class AdmitPatientCommandHandlerTests
{
    private readonly Mock<ResiliencePipelineProvider<string>> _pipelineProviderMock;

    public AdmitPatientCommandHandlerTests()
    {
        _pipelineProviderMock = new Mock<ResiliencePipelineProvider<string>>();

        _pipelineProviderMock
            .Setup(x => x.GetPipeline("sql-retry-pipeline"))
            .Returns(ResiliencePipeline.Empty);
    }

    [Fact]
    public async Task Handle_Success_ReturnsSuccessResponse()
    {
        // Arrange
        var mockMongo = new Mock<IPatientMongoRepository>();
        mockMongo.Setup(m => m.SaveResourceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                 .Returns(Task.CompletedTask);

        var mockCopayments = new Mock<IPatientSqlRepository>();
        mockCopayments.Setup(c => c.SaveTransactionAsync(It.IsAny<Copayment>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.Copayments).Returns(mockCopayments.Object);
        mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var mockLogger = new Mock<ILogger<AdmitPatientCommandHandler>>();

        var handler = new AdmitPatientCommandHandler(
            mockMongo.Object,
            mockUnitOfWork.Object,
            _pipelineProviderMock.Object,
            mockLogger.Object
        );

        var patientHistory = new PatientHistoryDto { Id = "123", ResourceType = "Patient", Copayment = 50 };
        var request = new AdmitPatientCommand(patientHistory, "{}");

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.True(response.Data);
        Assert.Contains(patientHistory.Id, response.ReturnMessage);
        mockMongo.Verify(m => m.SaveResourceAsync(patientHistory.ResourceType, patientHistory.Id, request.RawJson), Times.Once);
        mockCopayments.Verify(c => c.SaveTransactionAsync(It.Is<Copayment>(cp => cp.PatientDocument == patientHistory.Id && cp.CopaymentValue == patientHistory.Copayment), It.IsAny<CancellationToken>()), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SqlThrows_ReturnsFailureResponse()
    {
        // Arrange
        var mockMongo = new Mock<IPatientMongoRepository>();
        mockMongo.Setup(m => m.SaveResourceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                 .Returns(Task.CompletedTask);

        var mockCopayments = new Mock<IPatientSqlRepository>();
        mockCopayments.Setup(c => c.SaveTransactionAsync(It.IsAny<Copayment>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("SQL failure"));

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.Copayments).Returns(mockCopayments.Object);
        mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var mockLogger = new Mock<ILogger<AdmitPatientCommandHandler>>();

        var handler = new AdmitPatientCommandHandler(
            mockMongo.Object,
            mockUnitOfWork.Object,
            _pipelineProviderMock.Object,
            mockLogger.Object
        );

        var patientHistory = new PatientHistoryDto { Id = "456", ResourceType = "Patient", Copayment = 75 };
        var request = new AdmitPatientCommand(patientHistory, "{}");

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Contains("SQL failure", response.ReturnMessage);
        mockMongo.Verify(m => m.SaveResourceAsync(patientHistory.ResourceType, patientHistory.Id, request.RawJson), Times.Once);
        mockCopayments.Verify(c => c.SaveTransactionAsync(It.IsAny<Copayment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MongoThrows_ReturnsFailureResponse()
    {
        // Arrange
        var mockMongo = new Mock<IPatientMongoRepository>();
        mockMongo.Setup(m => m.SaveResourceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                 .ThrowsAsync(new Exception("Mongo failure"));

        var mockCopayments = new Mock<IPatientSqlRepository>();

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.Copayments).Returns(mockCopayments.Object);

        var mockLogger = new Mock<ILogger<AdmitPatientCommandHandler>>();

        var handler = new AdmitPatientCommandHandler(
            mockMongo.Object,
            mockUnitOfWork.Object,
            _pipelineProviderMock.Object,
            mockLogger.Object
        );

        var patientHistory = new PatientHistoryDto { Id = "789", ResourceType = "Patient", Copayment = 20 };
        var request = new AdmitPatientCommand(patientHistory, "{}");

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Contains("Mongo failure", response.ReturnMessage);
        mockMongo.Verify(m => m.SaveResourceAsync(patientHistory.ResourceType, patientHistory.Id, request.RawJson), Times.Once);
        mockCopayments.Verify(c => c.SaveTransactionAsync(It.IsAny<Copayment>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}