using Doppler.Push.Api.Contract;
using Doppler.Push.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
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
        private readonly Mock<IFirebaseCloudMessageService> _firebaseCloudMessageService = new Mock<IFirebaseCloudMessageService>();
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;

        public MessageControllerTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _firebaseCloudMessageService.Setup(s => s.SendMulticast(It.IsAny<PushNotificationDTO>()))
                .ReturnsAsync(new FirebaseMessageSendResponse { SuccessCount = 1, FailureCount = 0, Responses = null });
            _firebaseCloudMessageService.Setup(s => s.SendMulticastAsBatches(It.IsAny<PushNotificationDTO>()))
                .ReturnsAsync(new FirebaseMessageSendResponse { SuccessCount = 1, FailureCount = 0, Responses = null });
            _client = _factory
                .WithWebHostBuilder((e) =>
                    e.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<IFirebaseCloudMessageService>(s => _firebaseCloudMessageService.Object);
                    }))
                .CreateClient(new WebApplicationFactoryClientOptions());
        }

        [Fact]
        public async Task POST_messageSend_return_correct_status_when_contract_valid()
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
    }
}
