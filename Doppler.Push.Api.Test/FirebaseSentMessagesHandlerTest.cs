using AutoFixture;
using Doppler.Push.Api.Contract;
using Doppler.Push.Api.Services.FirebaseSentMessagesHandling;
using Flurl.Http.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.Push.Api
{
    public class FirebaseSentMessagesHandlerTest
    {
        private static readonly FirebaseSentMessagesHandlerSettings firebaseSentMessagesHandlerSettingsDefault =
            new FirebaseSentMessagesHandlerSettings
            {
                PushContactApiUrl = "https://localhost:5001",
                FatalMessagingErrorCodes = new List<int>() { 1, 2, 3, 4 }
            };

        private FirebaseSentMessagesHandler CreateSut(
            IOptions<FirebaseSentMessagesHandlerSettings> settings = null,
            IPushContactApiTokenGetter pushContactApiTokenGetter = null,
            ILogger<FirebaseSentMessagesHandler> logger = null)
        {
            return new FirebaseSentMessagesHandler(
                settings ?? Options.Create(firebaseSentMessagesHandlerSettingsDefault),
                pushContactApiTokenGetter ?? Mock.Of<IPushContactApiTokenGetter>(),
                logger ?? Mock.Of<ILogger<FirebaseSentMessagesHandler>>());
        }

        [Fact]
        public async Task HandleSentMessagesAsync_should_throw_argument_null_exception_when_firebase_message_send_response_is_null()
        {
            // Arrange
            FirebaseMessageSendResponse firebaseMessageSendResponse = null;

            var sut = CreateSut();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleSentMessagesAsync(firebaseMessageSendResponse));
        }

        [Fact]
        public async Task HandleSentMessagesAsync_should_does_not_call_push_contact_api_when_firebase_has_null_sent_responses()
        {
            // Arrange
            using var httpTest = new HttpTest();

            var firebaseMessageSendResponse = new FirebaseMessageSendResponse
            {
                SuccessCount = 0,
                FailureCount = 0,
                Responses = null
            };

            var sut = CreateSut();

            // Act
            await sut.HandleSentMessagesAsync(firebaseMessageSendResponse);

            // Assert
            httpTest.ShouldNotHaveMadeACall();
        }

        [Fact]
        public async Task HandleSentMessagesAsync_should_does_not_call_push_contact_api_when_firebase_has_empty_sent_responses()
        {
            // Arrange
            using var httpTest = new HttpTest();

            var firebaseMessageSendResponse = new FirebaseMessageSendResponse
            {
                SuccessCount = 0,
                FailureCount = 0,
                Responses = new List<FirebaseResponseItem>()
            };

            var sut = CreateSut();

            // Act
            await sut.HandleSentMessagesAsync(firebaseMessageSendResponse);

            // Assert
            httpTest.ShouldNotHaveMadeACall();
        }

        [Fact]
        public async Task HandleSentMessagesAsync_should_log_error_and_does_not_call_push_contact_api_when_push_contact_api_token_getter_throw_exception()
        {
            // Arrange
            using var httpTest = new HttpTest();

            var firebaseMessageSendResponse = new FirebaseMessageSendResponse
            {
                SuccessCount = 0,
                FailureCount = 1,
                Responses = new List<FirebaseResponseItem>
                {
                    new FirebaseResponseItem
                    {
                        IsSuccess = false,
                        Exception = new FirebaseExceptionItem
                        {
                            MessagingErrorCode = firebaseSentMessagesHandlerSettingsDefault.FatalMessagingErrorCodes.First()
                        }
                    }
                }
            };

            var expectedSentMessagesLogged = JsonSerializer.Serialize(firebaseMessageSendResponse.Responses);

            var pushContactApiTokenGetterMock = new Mock<IPushContactApiTokenGetter>();
            pushContactApiTokenGetterMock
                .Setup(x => x.GetTokenAsync())
                .ThrowsAsync(new Exception());

            var loggerMock = new Mock<ILogger<FirebaseSentMessagesHandler>>();

            var sut = CreateSut(
                pushContactApiTokenGetter: pushContactApiTokenGetterMock.Object,
                logger: loggerMock.Object
                );

            // Act
            await sut.HandleSentMessagesAsync(firebaseMessageSendResponse);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error handling following sent messages")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            httpTest.ShouldNotHaveMadeACall();
        }

        [Fact]
        public async Task HandleSentMessagesAsync_should_log_warning_message_when_has_not_handling_sent_messages()
        {
            // Arrange
            var fixture = new Fixture();

            var notHandlingSentMessages = new List<FirebaseResponseItem>
                {
                    new FirebaseResponseItem
                    {
                        IsSuccess = false,
                        MessageId = fixture.Create<string>(),
                        Exception = new FirebaseExceptionItem
                        {
                            MessagingErrorCode = 999
                        }
                    }
                };

            var firebaseMessageSendResponse = new FirebaseMessageSendResponse
            {
                SuccessCount = fixture.Create<int>(),
                FailureCount = fixture.Create<int>(),
                Responses = notHandlingSentMessages
            };

            var loggerMock = new Mock<ILogger<FirebaseSentMessagesHandler>>();

            var sut = CreateSut(logger: loggerMock.Object);

            // Act
            await sut.HandleSentMessagesAsync(firebaseMessageSendResponse);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Not handling for following Firebase sent messages")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task HandleSentMessagesAsync_should_does_not_call_push_contact_api_when_firebase_responses_has_not_fatal_messaging_error_codes()
        {
            // Arrange
            var fixture = new Fixture();

            using var httpTest = new HttpTest();

            var firebaseMessageSendResponse = new FirebaseMessageSendResponse
            {
                SuccessCount = fixture.Create<int>(),
                FailureCount = fixture.Create<int>(),
                Responses = new List<FirebaseResponseItem>
                {
                    new FirebaseResponseItem
                    {
                        IsSuccess = false,
                        Exception = new FirebaseExceptionItem
                        {
                            MessagingErrorCode = 999
                        }
                    }
                }
            };

            var sut = CreateSut();

            // Act
            await sut.HandleSentMessagesAsync(firebaseMessageSendResponse);

            // Assert
            httpTest.ShouldNotHaveMadeACall();
        }

        [Fact]
        public async Task HandleSentMessagesAsync_should_call_push_contact_api_delete_when_firebase_responses_has_fatal_messaging_error_codes()
        {
            // Arrange
            var fixture = new Fixture();

            using var httpTest = new HttpTest();

            var firebaseMessageSendResponse = new FirebaseMessageSendResponse
            {
                SuccessCount = fixture.Create<int>(),
                FailureCount = fixture.Create<int>(),
                Responses = new List<FirebaseResponseItem>
                {
                    new FirebaseResponseItem
                    {
                        IsSuccess = false,
                        Exception = new FirebaseExceptionItem
                        {
                            MessagingErrorCode = 1
                        }
                    }
                }
            };

            var sut = CreateSut();

            // Act
            await sut.HandleSentMessagesAsync(firebaseMessageSendResponse);

            // Assert
            httpTest.ShouldHaveCalled($"{firebaseSentMessagesHandlerSettingsDefault.PushContactApiUrl}/PushContact")
                .WithVerb(HttpMethod.Delete)
                .Times(1);
        }

        [Theory]
        [InlineData(500)]
        [InlineData(400)]
        [InlineData(401)]
        [InlineData(404)]
        public async Task HandleSentMessagesAsync_should_log_error_when_push_contact_api_response_not_success_status(int notSuccessStatus)
        {
            // Arrange
            var fixture = new Fixture();

            using var httpTest = new HttpTest();

            httpTest.RespondWith(fixture.Create<string>(), notSuccessStatus);

            var firebaseMessageSendResponse = new FirebaseMessageSendResponse
            {
                SuccessCount = fixture.Create<int>(),
                FailureCount = fixture.Create<int>(),
                Responses = new List<FirebaseResponseItem>
                {
                    new FirebaseResponseItem
                    {
                        IsSuccess = false,
                        Exception = new FirebaseExceptionItem
                        {
                            MessagingErrorCode = 1
                        }
                    }
                }
            };

            var loggerMock = new Mock<ILogger<FirebaseSentMessagesHandler>>();

            var sut = CreateSut(
                logger: loggerMock.Object
                );

            // Act
            await sut.HandleSentMessagesAsync(firebaseMessageSendResponse);

            // Assert
            httpTest.ShouldHaveMadeACall();
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error handling following sent messages")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
