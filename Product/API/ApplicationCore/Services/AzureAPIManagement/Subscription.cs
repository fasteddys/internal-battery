using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Services.AzureAPIManagement
{
    class Subscription
    {
        public string Id;
        public string OwnerId;
        public string Scope;
        public string Name;
        public string State;
        public DateTime? CreatedDate;
        public DateTime? StartDate;
        public DateTime? ExpirationDate;
        public DateTime? EndDate;
        public DateTime? NotificationDate;
        public string PrimaryKey;
        public string SecondaryKey;
        public string StateComment;
        public bool? AllowTracing;

        public string GetUserId() => OwnerId.Split('/').LastOrDefault();
    }
}
