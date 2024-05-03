using Doppler.Push.Api.Contract;
using System;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services
{
    public class DopplerMessageService : IMessageService
    {
        public DopplerMessageService()
        {
        }

        public async Task<MessageSendResponse> SendMulticast(PushNotificationDTO request)
        {
            await Task.Yield(); // it allows us to consider an async method without doing an operation
            throw new Exception("Not implemented");
        }

        public async Task<MessageSendResponse> SendMulticastAsBatches(PushNotificationDTO request)
        {
            await Task.Yield(); // it allows us to consider an async method without doing an operation
            throw new Exception("Not implemented");
        }

        public async Task<Device> GetDevice(string token)
        {
            await Task.Yield(); // it allows us to consider an async method without doing an operation
            throw new Exception("Not implemented");
        }
    }
}
