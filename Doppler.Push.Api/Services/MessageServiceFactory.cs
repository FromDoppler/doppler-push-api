using Microsoft.Extensions.DependencyInjection;
using System;

namespace Doppler.Push.Api.Services
{
    public class MessageServiceFactory : IMessageServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMessageService CreateFirebaseCloudMessageService()
        {
            return _serviceProvider.GetRequiredService<FirebaseCloudMessageService>();
        }

        public IMessageService CreateDopplerMessageService()
        {
            return _serviceProvider.GetRequiredService<DopplerMessageService>();
        }
    }
}
