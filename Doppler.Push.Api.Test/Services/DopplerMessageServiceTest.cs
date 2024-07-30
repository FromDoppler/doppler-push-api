using AutoFixture;
using Doppler.Push.Api.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.Push.Api.Services
{
    public class DopplerMessageServiceTest
    {
        private static DopplerMessageService CreateSut(
            IOptions<WebPushSettings> webPushSettings = null,
            IWebPushClient webPushClient = null,
            ILogger<DopplerMessageService> logger = null
        )
        {
            return new DopplerMessageService(
                webPushSettings ?? Mock.Of<IOptions<WebPushSettings>>(),
                webPushClient ?? Mock.Of<IWebPushClient>(),
                logger ?? Mock.Of<ILogger<DopplerMessageService>>()
            );
        }

        public static IEnumerable<object[]> SubscriptionsData()
        {
            var fixture = new Fixture();
            var s1 = fixture.Create<SubscriptionDTO>();
            var r1 = new ResponseItem()
            {
                Subscription = new SubscriptionResponseDTO()
                {
                    Endpoint = s1.Endpoint,
                    Auth = s1.Auth,
                    P256DH = s1.P256DH,
                },
                IsSuccess = true,
                Exception = null
            };

            var s2 = fixture.Create<SubscriptionDTO>();
            var r2 = new ResponseItem()
            {
                Subscription = new SubscriptionResponseDTO()
                {
                    Endpoint = s2.Endpoint,
                    Auth = s2.Auth,
                    P256DH = s2.P256DH,
                },
                IsSuccess = false,
                Exception = new ExceptionItem() { Message = "Error in s2" },
            };

            yield return new object[] { s1, r1, true };
            yield return new object[] { s2, r2, false };
        }

        [Theory]
        [MemberData(nameof(SubscriptionsData))]
        public async Task SendMulticast_should_return_a_reponse_with_correct_success_and_failure_quantity(
            SubscriptionDTO subscription,
            ResponseItem responseItem,
            bool subscriptionProcessedOk
        )
        {
            // Arrange
            var fixture = new Fixture();

            var pushNotification = fixture.Create<PushNotificationDTO>();
            pushNotification.Subscriptions = new SubscriptionDTO[] { subscription };

            var webPushSettingsMock = fixture.Create<WebPushSettings>();

            var webPushClientMock = new Mock<IWebPushClient>();
            webPushClientMock
                .Setup(x => x.SetVapidDetails(webPushSettingsMock.Subject, webPushSettingsMock.PublicKey, webPushSettingsMock.PrivateKey));
            webPushClientMock
                .Setup(x => x.SendNotificationAsync(subscription, It.IsAny<string>(), default, default))
                .ReturnsAsync(responseItem);

            var sut = CreateSut(Options.Create(webPushSettingsMock), webPushClientMock.Object);

            // Act
            var response = await sut.SendMulticast(pushNotification);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.SuccessCount == (subscriptionProcessedOk ? 1 : 0));
            Assert.True(response.FailureCount == (subscriptionProcessedOk ? 0 : 1));

            var firstResponseFromResponses = response.Responses.First();
            if (subscriptionProcessedOk)
            {
                Assert.Null(firstResponseFromResponses.Exception);
            }
            else
            {
                Assert.NotNull(firstResponseFromResponses.Exception);
            }
        }

        [Fact]
        public async Task SendMulticast_should_return_a_reponse_with_BadRequest_statuscode()
        {
            // Arrange
            var fixture = new Fixture();
            var subscription = fixture.Create<SubscriptionDTO>();

            var expectedExceptionMessage = "An Argument Exception";

            var pushNotification = fixture.Create<PushNotificationDTO>();
            pushNotification.Subscriptions = new SubscriptionDTO[] { subscription };

            var webPushSettingsMock = fixture.Create<WebPushSettings>();

            var webPushClientMock = new Mock<IWebPushClient>();
            webPushClientMock
                .Setup(x => x.SetVapidDetails(webPushSettingsMock.Subject, webPushSettingsMock.PublicKey, webPushSettingsMock.PrivateKey));
            webPushClientMock
                .Setup(x => x.SendNotificationAsync(subscription, It.IsAny<string>(), default, default))
                .ThrowsAsync(new ArgumentException(expectedExceptionMessage));

            var sut = CreateSut(Options.Create(webPushSettingsMock), webPushClientMock.Object);

            // Act
            var response = await sut.SendMulticast(pushNotification);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.SuccessCount == 0);
            Assert.True(response.FailureCount == 1);

            var firstSubscriptionResponse = response.Responses.First();
            Assert.NotNull(firstSubscriptionResponse);
            Assert.Equal(subscription.Endpoint, firstSubscriptionResponse.Subscription.Endpoint);
            Assert.Equal(subscription.Auth, firstSubscriptionResponse.Subscription.Auth);
            Assert.Equal(subscription.P256DH, firstSubscriptionResponse.Subscription.P256DH);

            var responseSubscriptionException = firstSubscriptionResponse.Exception;
            Assert.NotNull(responseSubscriptionException);
            Assert.Equal(expectedExceptionMessage, responseSubscriptionException.Message);
            Assert.Equal((int)HttpStatusCode.BadRequest, responseSubscriptionException.MessagingErrorCode);
        }

        [Fact]
        public async Task SendMulticast_should_return_a_reponse_with_InternalServerError__statuscode()
        {
            // Arrange
            var fixture = new Fixture();
            var subscription = fixture.Create<SubscriptionDTO>();

            var expectedExceptionMessage = "An Argument Exception";

            var pushNotification = fixture.Create<PushNotificationDTO>();
            pushNotification.Subscriptions = new SubscriptionDTO[] { subscription };

            var webPushSettingsMock = fixture.Create<WebPushSettings>();

            var webPushClientMock = new Mock<IWebPushClient>();
            webPushClientMock
                .Setup(x => x.SetVapidDetails(webPushSettingsMock.Subject, webPushSettingsMock.PublicKey, webPushSettingsMock.PrivateKey));
            webPushClientMock
                .Setup(x => x.SendNotificationAsync(subscription, It.IsAny<string>(), default, default))
                .ThrowsAsync(new Exception(expectedExceptionMessage));

            var sut = CreateSut(Options.Create(webPushSettingsMock), webPushClientMock.Object);

            // Act
            var response = await sut.SendMulticast(pushNotification);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.SuccessCount == 0);
            Assert.True(response.FailureCount == 1);

            var firstSubscriptionResponse = response.Responses.First();
            Assert.NotNull(firstSubscriptionResponse);
            Assert.Equal(subscription.Endpoint, firstSubscriptionResponse.Subscription.Endpoint);
            Assert.Equal(subscription.Auth, firstSubscriptionResponse.Subscription.Auth);
            Assert.Equal(subscription.P256DH, firstSubscriptionResponse.Subscription.P256DH);

            var responseSubscriptionException = firstSubscriptionResponse.Exception;
            Assert.NotNull(responseSubscriptionException);
            Assert.Equal((int)HttpStatusCode.InternalServerError, responseSubscriptionException.MessagingErrorCode);
        }
    }
}
