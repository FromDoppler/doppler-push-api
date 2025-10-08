using System.Collections.Generic;
using System.Linq;

namespace Doppler.Push.Api.Contract
{
    public class ActionModel
    {
        public string Action { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
    }

    public class MessageSendRequest
    {
        public string NotificationTitle { get; set; }
        public string NotificationBody { get; set; }
        public string NotificationOnClickLink { get; set; }
        public string ImageUrl { get; set; }
        public string IconUrl { get; set; }
        public List<ActionModel> Actions { get; set; }
        public bool HasActions => Actions != null && Actions.Any();
    }
}
