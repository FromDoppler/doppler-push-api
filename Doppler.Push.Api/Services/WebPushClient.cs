using Doppler.Push.Api.Contract;
using Doppler.Push.Api.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services
{
    public class WebPushClient : IWebPushClient
    {
        // default TTL is 4 weeks.
        private const int DefaultTtl = 2419200;
        private readonly HttpClientHandler _httpClientHandler;

        private HttpClient _httpClient;
        private VapidDetails _vapidDetails;

        // Used so we only cleanup internally created http clients
        private bool _isHttpClientInternallyCreated;

        public WebPushClient()
        {
        }

        public WebPushClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public WebPushClient(HttpClientHandler httpClientHandler)
        {
            _httpClientHandler = httpClientHandler;
        }

        protected HttpClient HttpClient
        {
            get
            {
                if (_httpClient != null)
                {
                    return _httpClient;
                }

                _isHttpClientInternallyCreated = true;
                _httpClient = _httpClientHandler == null
                    ? new HttpClient()
                    : new HttpClient(_httpClientHandler);

                return _httpClient;
            }
        }

        /// <summary>
        ///     When marking requests where you want to define VAPID details, call this method
        ///     before sendNotifications() or pass in the details and options to
        ///     sendNotification.
        /// </summary>
        /// <param name="subject">This must be either a URL or a 'mailto:' address</param>
        /// <param name="publicKey">The public VAPID key as a base64 encoded string</param>
        /// <param name="privateKey">The private VAPID key as a base64 encoded string</param>
        public void SetVapidDetails(string subject, string publicKey, string privateKey)
        {
            VapidHelper.ValidateSubject(subject);
            VapidHelper.ValidatePublicKey(publicKey);
            VapidHelper.ValidatePrivateKey(privateKey);

            _vapidDetails = new VapidDetails()
            {
                Subject = subject,
                PublicKey = publicKey,
                PrivateKey = privateKey,
            };
        }

        /// <summary>
        ///     To send a push notification asynchronous call this method with a subscription, optional payload and any options
        ///     Will exception if unsuccessful
        /// </summary>
        /// <param name="subscription">The PushSubscription you wish to send the notification to.</param>
        /// <param name="payload">The payload you wish to send to the user</param>
        /// <param name="options">
        ///     Options for the GCM API key and vapid keys can be passed in if they are unique for each
        ///     notification.
        /// </param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        public async Task<ResponseItem> SendNotificationAsync
        (
            SubscriptionDTO subscription,
            string payload = null,
            Dictionary<string, object> options = null,
            CancellationToken cancellationToken = default
        )
        {
            var request = GenerateRequestDetails(subscription, payload, options);
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            return await HandleResponse(response, subscription);
        }

        /// <summary>
        ///     To get a request without sending a push notification call this method.
        ///     This method will throw an ArgumentException if there is an issue with the input.
        /// </summary>
        /// <param name="subscription">The Subscription you wish to send the notification to.</param>
        /// <param name="payload">The payload you wish to send to the user</param>
        /// <param name="options">
        ///     Options for the GCM API key and vapid keys can be passed in if they are unique for each
        ///     notification.
        /// </param>
        /// <returns>A HttpRequestMessage object that can be sent.</returns>
        private HttpRequestMessage GenerateRequestDetails(SubscriptionDTO subscription, string payload,
            Dictionary<string, object> options = null)
        {
            if (!Uri.IsWellFormedUriString(subscription.Endpoint, UriKind.Absolute))
            {
                throw new ArgumentException(@"You must pass in a subscription with at least a valid endpoint");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, subscription.Endpoint);

            if (!string.IsNullOrEmpty(payload) &&
                (string.IsNullOrEmpty(subscription.Auth) || string.IsNullOrEmpty(subscription.P256DH)))
            {
                throw new ArgumentException(@"To send a message with a payload, the subscription must have 'auth' and 'p256dh' keys.");
            }

            var currentVapidDetails = _vapidDetails;
            var timeToLive = DefaultTtl;
            var extraHeaders = new Dictionary<string, object>();

            if (options != null)
            {
                var validOptionsKeys = new List<string> { "headers", "vapidDetails", "TTL" };
                foreach (var key in options.Keys)
                {
                    if (!validOptionsKeys.Contains(key))
                    {
                        throw new ArgumentException(key + " is an invalid options. The valid options are" + string.Join(",", validOptionsKeys));
                    }
                }

                if (options.ContainsKey("headers"))
                {
                    var headers = options["headers"] as Dictionary<string, object>;

                    extraHeaders = headers ?? throw new ArgumentException("options.headers must be of type Dictionary<string,object>");
                }

                if (options.ContainsKey("vapidDetails"))
                {
                    var vapidDetails = options["vapidDetails"] as VapidDetails;
                    currentVapidDetails = vapidDetails ?? throw new ArgumentException("options.vapidDetails must be of type VapidDetails");
                }

                if (options.ContainsKey("TTL"))
                {
                    var ttl = options["TTL"] as int?;
                    if (ttl == null)
                    {
                        throw new ArgumentException("options.TTL must be of type int");
                    }

                    //at this stage ttl cannot be null.
                    timeToLive = (int)ttl;
                }
            }

            string cryptoKeyHeader = null;
            request.Headers.Add("TTL", timeToLive.ToString());

            foreach (var header in extraHeaders)
            {
                request.Headers.Add(header.Key, header.Value.ToString());
            }

            if (!string.IsNullOrEmpty(payload))
            {
                var encryptedPayload = EncryptPayload(subscription, payload);

                request.Content = new ByteArrayContent(encryptedPayload.Payload);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                request.Content.Headers.ContentLength = encryptedPayload.Payload.Length;
                request.Content.Headers.ContentEncoding.Add("aesgcm");
                request.Headers.Add("Encryption", "salt=" + encryptedPayload.Base64EncodeSalt());
                cryptoKeyHeader = @"dh=" + encryptedPayload.Base64EncodePublicKey();
            }
            else
            {
                request.Content = new ByteArrayContent(new byte[0]);
                request.Content.Headers.ContentLength = 0;
            }

            if (currentVapidDetails != null)
            {
                var uri = new Uri(subscription.Endpoint);
                var audience = uri.Scheme + @"://" + uri.Host;

                var vapidHeaders = VapidHelper.GetVapidHeaders(
                    audience,
                    currentVapidDetails.Subject,
                    currentVapidDetails.PublicKey,
                    currentVapidDetails.PrivateKey,
                    currentVapidDetails.Expiration
                );
                request.Headers.Add(@"Authorization", vapidHeaders["Authorization"]);
                if (string.IsNullOrEmpty(cryptoKeyHeader))
                {
                    cryptoKeyHeader = vapidHeaders["Crypto-Key"];
                }
                else
                {
                    cryptoKeyHeader += @";" + vapidHeaders["Crypto-Key"];
                }
            }

            request.Headers.Add("Crypto-Key", cryptoKeyHeader);
            return request;
        }

        private static EncryptionResult EncryptPayload(SubscriptionDTO subscription, string payload)
        {
            try
            {
                return Encryptor.Encrypt(subscription.P256DH, subscription.Auth, payload);
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is ArgumentException)
                {
                    throw new ArgumentException("Unable to encrypt the payload with the encryption key of this subscription.");
                }

                throw;
            }
        }

        /// <summary>
        ///     Handle Web Push responses.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="subscription"></param>
        private static async Task<ResponseItem> HandleResponse(HttpResponseMessage response, SubscriptionDTO subscription)
        {
            // Successful
            if (response.IsSuccessStatusCode)
            {
                return new ResponseItem()
                {
                    IsSuccess = true,
                    Subscription = subscription,
                };
            }

            // Error
            var responseCodeMessage = @"Received unexpected response code: " + (int)response.StatusCode;
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    responseCodeMessage = "Bad Request";
                    break;

                case HttpStatusCode.RequestEntityTooLarge:
                    responseCodeMessage = "Payload too large";
                    break;

                case (HttpStatusCode)429:
                    responseCodeMessage = "Too many request";
                    break;

                case HttpStatusCode.NotFound:
                case HttpStatusCode.Gone:
                    responseCodeMessage = "Subscription no longer valid";
                    break;
            }

            string details = null;
            if (response.Content != null)
            {
                details = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var message = string.IsNullOrEmpty(details)
                ? responseCodeMessage
                : $"{responseCodeMessage}. Details: {details}";

            return new ResponseItem()
            {
                IsSuccess = false,
                Exception = new ExceptionItem()
                {
                    Message = message,
                    MessagingErrorCode = (int)response.StatusCode,
                },
                Subscription = subscription,
            };
        }

        public void Dispose()
        {
            if (_httpClient != null && _isHttpClientInternallyCreated)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
        }
    }
}
