using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class EmailVerification
    {
        public EmailVerification()
        {
            EmailVerificationGuid = Guid.NewGuid();
            Token = Guid.NewGuid();
            ExpirationDateTime = DateTime.UtcNow.AddMinutes(5);
            CreatedDate = DateTime.UtcNow;
            ModifyDate = DateTime.UtcNow;
        }

        public void RefreshToken()
        {
            ExpirationDateTime = DateTime.UtcNow.AddMinutes(5);
            Token = Guid.NewGuid();
            ModifyDate = DateTime.UtcNow;
        }

        public int EmailVerificationId { get; set; }
        public Guid EmailVerificationGuid { get; set; }
        public int SubscriberId { get; set; }
        public Guid Token { get; set; }
        public DateTime ExpirationDateTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifyDate { get; set; }
    }
}
