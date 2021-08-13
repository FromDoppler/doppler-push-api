using AutoFixture;
using Doppler.Push.Api.Contract;
using Doppler.Push.Api.Controllers;
using Doppler.Push.Api.Services;
using Doppler.Push.Api.Services.FirebaseSentMessagesHandling;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.Push.Api
{
    public class MessageControllerTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        const string TOKEN_SUPERUSER_VALID = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjIwMDAwMDAwMDB9.rUtvRqMxrnQzVHDuAjgWa2GJAJwZ-wpaxqdjwP7gmVa7XJ1pEmvdTMBdirKL5BJIE7j2_hsMvEOKUKVjWUY-IE0e0u7c82TH0l_4zsIztRyHMKtt9QE9rBRQnJf8dcT5PnLiWkV_qEkpiIKQ-wcMZ1m7vQJ0auEPZyyFBKmU2caxkZZOZ8Kw_1dx-7lGUdOsUYad-1Rt-iuETGAFijQrWggcm3kV_KmVe8utznshv2bAdLJWydbsAUEfNof0kZK5Wu9A80DJd3CRiNk8mWjQxF_qPOrGCANOIYofhB13yuYi48_8zVPYku-llDQjF77BmQIIIMrCXs8IMT3Lksdxuw";
        private readonly Mock<IFirebaseCloudMessageService> _firebaseCloudMessageService = new Mock<IFirebaseCloudMessageService>();
        private readonly Mock<IFirebaseSentMessagesHandler> _firebaseSentMessagesHandler = new Mock<IFirebaseSentMessagesHandler>();
        private readonly Mock<ILogger<MessageController>> _loggerMock = new Mock<ILogger<MessageController>>();
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;

        public MessageControllerTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _firebaseCloudMessageService.Setup(s => s.SendMulticast(It.IsAny<FirebaseMessageSendRequest>()))
                .ReturnsAsync(new FirebaseMessageSendResponse { SuccessCount = 1, FailureCount = 0, Responses = null });
            _firebaseSentMessagesHandler.Setup(s => s.HandleSentMessagesAsync(It.IsAny<FirebaseMessageSendResponse>()))
                .Returns(Task.CompletedTask);
            _client = _factory
                .WithWebHostBuilder((e) =>
                    e.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<IFirebaseCloudMessageService>(s => _firebaseCloudMessageService.Object);
                        services.AddSingleton<IFirebaseSentMessagesHandler>(s => _firebaseSentMessagesHandler.Object);
                        services.AddSingleton<ILogger<MessageController>>(s => _loggerMock.Object);
                    }))
                .CreateClient(new WebApplicationFactoryClientOptions());
        }

        [Fact]
        public async Task POST_messageSend_return_correct_status_when_contract_valid_and_handling_sent_messages_not_throw_exception()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TOKEN_SUPERUSER_VALID);
            var messageSendRequest = new { NotificationTitle = "Title", NotificationBody = "body", Tokens = new string[] { } };
            var stringContent = new StringContent(JsonConvert.SerializeObject(messageSendRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/message", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error handling sent messages"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }

        [Fact]
        public async Task POST_messageSend_should_return_ok_when_contract_is_valid_and_handling_sent_messages_throw_exception()
        {
            // Arrange
            var fixture = new Fixture();

            var firebaseMessageSendResponse = fixture.Create<FirebaseMessageSendResponse>();

            var firebaseCloudMessageService = new Mock<IFirebaseCloudMessageService>();
            firebaseCloudMessageService
                .Setup(s => s.SendMulticast(It.IsAny<FirebaseMessageSendRequest>()))
                .ReturnsAsync(firebaseMessageSendResponse);

            var firebaseSentMessagesHandler = new Mock<IFirebaseSentMessagesHandler>();
            firebaseSentMessagesHandler.Setup(s => s.HandleSentMessagesAsync(firebaseMessageSendResponse))
                .ThrowsAsync(new Exception());

            var loggerMock = new Mock<ILogger<MessageController>>();

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(firebaseCloudMessageService.Object);
                    services.AddSingleton(firebaseSentMessagesHandler.Object);
                    services.AddSingleton(loggerMock.Object);
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Post, "message")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_SUPERUSER_VALID}" } },
                Content = JsonContent.Create(fixture.Create<FirebaseMessageSendRequest>())
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error handling sent messages"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
