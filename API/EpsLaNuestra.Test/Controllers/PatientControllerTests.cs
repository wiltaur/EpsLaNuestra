using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using EpsLaNuestra.API.Controllers;
using EpsLaNuestra.Application.Features.Patients.Commands;
using EpsLaNuestra.Application.Wrappers;
using MediatR;
using EpsLaNuestra.Application.Utilities;

namespace EpsLaNuestra.Test.Controllers
{
    public class PatientControllerTests
    {
        private static JsonElement BuildSampleJson()
        {
            var json = """
            {
              "resourceType": "Patient",
              "id": "1",
              "patient": {
                "numberId": "123",
                "name": "John Doe"
              },
              "paymentData": {
                "copayment": 20
              }
            }
            """;
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }

        [Fact]
        public async Task AdmitPatient_ReturnsBadRequest_WhenValidationFails()
        {
            // Arrange
            var rawJson = BuildSampleJson();

            var validatorMock = new Mock<IValidator<PatientJsonWrapper>>();
            var failures = new[] { new ValidationFailure("RawJson", "Invalid payload") };
            var invalidResult = new ValidationResult(failures);
            validatorMock
                .Setup(v => v.Validate(It.IsAny<PatientJsonWrapper>()))
                .Returns(invalidResult);

            var mediatorMock = new Mock<IMediator>();

            var controller = new PatientController(mediatorMock.Object);

            // Act
            var actionResult = await controller.AdmitPatient(rawJson, validatorMock.Object);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public async Task AdmitPatient_ReturnsOk_WhenMediatorReturnsSuccess()
        {
            // Arrange
            var rawJson = BuildSampleJson();

            var validatorMock = new Mock<IValidator<PatientJsonWrapper>>();
            validatorMock
                .Setup(v => v.Validate(It.IsAny<PatientJsonWrapper>()))
                .Returns(new ValidationResult());

            var mediatorMock = new Mock<IMediator>();

            var responseInstance = new ApiResponseUtility<bool>(true)
            {
                IsSuccess = true,
                Data = true
            };

            mediatorMock
                .Setup(m => m.Send(It.IsAny<AdmitPatientCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseInstance);

            var controller = new PatientController(mediatorMock.Object);

            // Act
            var actionResult = await controller.AdmitPatient(rawJson, validatorMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Same(responseInstance, okResult.Value);
        }

        [Fact]
        public async Task AdmitPatient_ReturnsInternalServerError_WhenMediatorReturnsFailure()
        {
            // Arrange
            var rawJson = BuildSampleJson();

            var validatorMock = new Mock<IValidator<PatientJsonWrapper>>();
            validatorMock
                .Setup(v => v.Validate(It.IsAny<PatientJsonWrapper>()))
                .Returns(new ValidationResult());

            var mediatorMock = new Mock<IMediator>();

            var responseInstance = new ApiResponseUtility<bool>(false)
            {
                IsSuccess = false,
                Data = false
            };

            mediatorMock
                .Setup(m => m.Send(It.IsAny<AdmitPatientCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseInstance);

            var controller = new PatientController(mediatorMock.Object);

            // Act
            var actionResult = await controller.AdmitPatient(rawJson, validatorMock.Object);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Same(responseInstance, objectResult.Value);
        }
    }
}