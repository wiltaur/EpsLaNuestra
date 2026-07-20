using Moq;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using Polly;
using EpsLaNuestra.Application.Features.Patients.Commands;
using EpsLaNuestra.Domain.Interfaces;
using EpsLaNuestra.Domain.Interfaces.Repositories;
using EpsLaNuestra.Domain.Entities;
using EpsLaNuestra.Domain.DTOs;

namespace EpsLaNuestra.Test.Commands;

public class AdmitPatientCommandHandlerTests
{
    private readonly Mock<ResiliencePipelineProvider<string>> _pipelineProviderMock;
    private readonly Mock<IPatientMongoRepository> _mockMongoRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IBlazorConnectUtility> _mockConnectUtility;
    private readonly Mock<ILogger<AdmitPatientCommandHandler>> _mockLogger;
    private AdmitPatientCommandHandler admitPatient;

    public AdmitPatientCommandHandlerTests()
    {
        _mockMongoRepository = new Mock<IPatientMongoRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockConnectUtility = new Mock<IBlazorConnectUtility>();
        _mockLogger = new Mock<ILogger<AdmitPatientCommandHandler>>();
        _pipelineProviderMock = new Mock<ResiliencePipelineProvider<string>>();

        _pipelineProviderMock
            .Setup(x => x.GetPipeline("sql-retry-pipeline"))
            .Returns(ResiliencePipeline.Empty);

        admitPatient = new AdmitPatientCommandHandler(
            _mockMongoRepository.Object,
            _mockUnitOfWork.Object,
            _pipelineProviderMock.Object,
            _mockLogger.Object,
            _mockConnectUtility.Object
        );
    }

    [Fact]
    public async Task Handle_Success_ReturnsSuccessResponse()
    {
        // Arrange
        _mockMongoRepository.Setup(m => m.SaveResourceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                 .Returns(Task.CompletedTask);

        var mockCopayments = new Mock<IPatientSqlRepository>();
        mockCopayments.Setup(c => c.SaveTransactionAsync(It.IsAny<Copayment>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(true));

        _mockUnitOfWork.Setup(u => u.Copayments).Returns(mockCopayments.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var mockLogger = new Mock<ILogger<AdmitPatientCommandHandler>>();

        var patientHistory = new PatientHistoryDto { Id = "123", ResourceType = "Patient", Copayment = 50 };
        var request = new AdmitPatientCommand(patientHistory, "{}");

        // Act
        var response = await admitPatient.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.True(response.Data);
        Assert.Contains(patientHistory.Id, response.ReturnMessage);
        _mockMongoRepository.Verify(m => m.SaveResourceAsync(patientHistory.ResourceType, patientHistory.Id, request.RawJson), Times.Once);
        mockCopayments.Verify(c => c.SaveTransactionAsync(It.IsAny<Copayment>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SqlThrows_ReturnsFailureResponse()
    {
        // Arrange
        _mockMongoRepository.Setup(m => m.SaveResourceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                 .Returns(Task.CompletedTask);

        var mockCopayments = new Mock<IPatientSqlRepository>();
        mockCopayments.Setup(c => c.SaveTransactionAsync(It.IsAny<Copayment>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("SQL failure"));

        _mockUnitOfWork.Setup(u => u.Copayments).Returns(mockCopayments.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var mockLogger = new Mock<ILogger<AdmitPatientCommandHandler>>();

        var patientHistory = new PatientHistoryDto { Id = "456", ResourceType = "Patient", Copayment = 75 };
        var request = new AdmitPatientCommand(patientHistory, "{}");

        // Act
        var response = await admitPatient.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Contains("SQL failure", response.ReturnMessage);
        _mockMongoRepository.Verify(m => m.SaveResourceAsync(patientHistory.ResourceType, patientHistory.Id, request.RawJson), Times.Once);
        mockCopayments.Verify(c => c.SaveTransactionAsync(It.IsAny<Copayment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MongoThrows_ReturnsFailureResponse()
    {
        // Arrange
        _mockMongoRepository.Setup(m => m.SaveResourceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                 .ThrowsAsync(new Exception("Mongo failure"));

        var mockCopayments = new Mock<IPatientSqlRepository>();

        _mockUnitOfWork.Setup(u => u.Copayments).Returns(mockCopayments.Object);

        var mockLogger = new Mock<ILogger<AdmitPatientCommandHandler>>();

        var patientHistory = new PatientHistoryDto { Id = "789", ResourceType = "Patient", Copayment = 20 };
        var request = new AdmitPatientCommand(patientHistory, "{}");

        // Act
        var response = await admitPatient.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Contains("Mongo failure", response.ReturnMessage);
        _mockMongoRepository.Verify(m => m.SaveResourceAsync(patientHistory.ResourceType, patientHistory.Id, request.RawJson), Times.Once);
        mockCopayments.Verify(c => c.SaveTransactionAsync(It.IsAny<Copayment>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}