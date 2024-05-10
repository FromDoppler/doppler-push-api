using AutoFixture;
using Doppler.Push.Api.Contract;
using Doppler.Push.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.Push.Api
{
    public class MessageControllerTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        const string TOKEN_SUPERUSER_VALID = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjIwMDAwMDAwMDB9.rUtvRqMxrnQzVHDuAjgWa2GJAJwZ-wpaxqdjwP7gmVa7XJ1pEmvdTMBdirKL5BJIE7j2_hsMvEOKUKVjWUY-IE0e0u7c82TH0l_4zsIztRyHMKtt9QE9rBRQnJf8dcT5PnLiWkV_qEkpiIKQ-wcMZ1m7vQJ0auEPZyyFBKmU2caxkZZOZ8Kw_1dx-7lGUdOsUYad-1Rt-iuETGAFijQrWggcm3kV_KmVe8utznshv2bAdLJWydbsAUEfNof0kZK5Wu9A80DJd3CRiNk8mWjQxF_qPOrGCANOIYofhB13yuYi48_8zVPYku-llDQjF77BmQIIIMrCXs8IMT3Lksdxuw";
        private readonly Mock<IMessageService> _firebaseCloudMessageService = new Mock<IMessageService>();
        private readonly Mock<IMessageService> _dopplerMessageService = new Mock<IMessageService>();
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;

        public MessageControllerTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;

            _firebaseCloudMessageService.Setup(s => s.SendMulticast(It.IsAny<PushNotificationDTO>()))
                .ReturnsAsync(new MessageSendResponse { SuccessCount = 1, FailureCount = 0, Responses = null });
            _firebaseCloudMessageService.Setup(s => s.SendMulticastAsBatches(It.IsAny<PushNotificationDTO>()))
                .ReturnsAsync(new MessageSendResponse { SuccessCount = 1, FailureCount = 0, Responses = null });

            _dopplerMessageService.Setup(s => s.SendMulticast(It.IsAny<PushNotificationDTO>()))
                .ReturnsAsync(new MessageSendResponse { SuccessCount = 1, FailureCount = 0, Responses = null });

            var messageServiceFactoryMock = new Mock<IMessageServiceFactory>();
            messageServiceFactoryMock.Setup(f => f.CreateFirebaseCloudMessageService()).Returns(_firebaseCloudMessageService.Object);
            messageServiceFactoryMock.Setup(f => f.CreateDopplerMessageService()).Returns(_dopplerMessageService.Object);

            _client = _factory
                .WithWebHostBuilder((e) =>
                    e.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<IMessageServiceFactory>(_ => messageServiceFactoryMock.Object);
                        services.AddSingleton<IMessageService>(s => _firebaseCloudMessageService.Object);
                        services.AddSingleton<IMessageService>(s => _dopplerMessageService.Object);
                    }))
                .CreateClient(new WebApplicationFactoryClientOptions());
        }

        [Fact]
        public async Task POST_messageSend_return_OK_when_contract_valid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TOKEN_SUPERUSER_VALID);
            var messageSendRequest = new { NotificationTitle = "Title", NotificationBody = "body", Tokens = new string[] { } };
            var stringContent = new StringContent(JsonConvert.SerializeObject(messageSendRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/message", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task POST_SendWebPush_return_OK_when_contract_valid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TOKEN_SUPERUSER_VALID);

            var fixture = new Fixture();
            var subscription = fixture.Create<Subscription>();

            var pushNotificationRequest = new
            {
                NotificationTitle = "title",
                NotificationBody = "body",
                Subscriptions = new[] { subscription }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(pushNotificationRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/webpush", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static IEnumerable<object[]> NullOrEmptySubscriptionsData()
        {
            yield return new object[] { null };
            yield return new object[] { new Subscription[] { } };
        }

        [Theory]
        [MemberData(nameof(NullOrEmptySubscriptionsData))]
        public async Task POST_SendWebPush_return_BadRequest_when_subscriptions_are_null_or_empty(Subscription[] subscriptions)
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TOKEN_SUPERUSER_VALID);

            var pushNotificationRequest = new
            {
                NotificationTitle = "title",
                NotificationBody = "body",
                Subscriptions = subscriptions
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(pushNotificationRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/webpush", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
