using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using EpsLaNuestra.API.Controllers;

namespace EpsLaNuestra.Test.Controllers
{
    public class PatientControllerTests
    {
        [Fact]
        public async Task AdmitPatient_ReturnsBadRequest_WhenBodyNotJsonObject()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var controller = new PatientController(mockMediator.Object);

            using var doc = JsonDocument.Parse("123");
            JsonElement rawJson = doc.RootElement;

            // Act
            var actionResult = await controller.AdmitPatient(rawJson);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.NotNull(badRequestResult.Value);

            mockMediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AdmitPatient_ReturnsBadRequest_WhenMissingRequiredProperties()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var controller = new PatientController(mockMediator.Object);

            string json = @"{
                ""resourceType"": ""Patient"",
                ""id"": ""patient-123"",
                ""paymentData"": {
                    ""someOtherField"": 1
                }
            }";
            using var doc = JsonDocument.Parse(json);
            JsonElement rawJson = doc.RootElement;

            // Act
            var actionResult = await controller.AdmitPatient(rawJson);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.NotNull(badRequestResult.Value);

            mockMediator.VerifyNoOtherCalls();
        }
    }
}