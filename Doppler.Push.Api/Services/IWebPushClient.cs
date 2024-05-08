using Doppler.Push.Api.Contract;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services
{
    public interface IWebPushClient : IDisposable
    {
        /// <summary>
        ///     When marking requests where you want to define VAPID details, call this method
        ///     before sendNotifications() or pass in the details and options to
        ///     sendNotification.
        /// </summary>
        /// <param name="subject">This must be either a URL or a 'mailto:' address</param>
        /// <param name="publicKey">The public VAPID key as a base64 encoded string</param>
        /// <param name="privateKey">The private VAPID key as a base64 encoded string</param>
        void SetVapidDetails(string subject, string publicKey, string privateKey);

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
        Task SendNotificationAsync(SubscriptionDTO subscription, string payload = null, Dictionary<string, object> options = null, CancellationToken cancellationToken = default);
    }
}
