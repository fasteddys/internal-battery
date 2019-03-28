using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class EmailVerification
    {
        public int EmailVerificationId { get; set; }
        public Guid EmailVerificationGuid { get; set; }
        public int SubscriberId { get; set; }
        public Guid Token { get; set; }
        public DateTime ExpirationDateTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifyDate { get; set; }

        public EmailVerification() { }

        public EmailVerification(int tokenLifetimeMinutes)
        {
            EmailVerificationGuid = Guid.NewGuid();
            Token = Guid.NewGuid();
            ExpirationDateTime = DateTime.UtcNow.AddMinutes(tokenLifetimeMinutes);
            CreatedDate = DateTime.UtcNow;
            ModifyDate = DateTime.UtcNow;
        }

        public void RefreshToken(int tokenLifetimeMinutes)
        {
            ExpirationDateTime = DateTime.UtcNow.AddMinutes(tokenLifetimeMinutes);
            Token = Guid.NewGuid();
            ModifyDate = DateTime.UtcNow;
        }

        public static void SetSubscriberEmailVerification(Subscriber subscriber, int tokenTtlMinutes)
        {
            // create or reset/refresh token
            if (subscriber.EmailVerification == null)
                subscriber.EmailVerification = new EmailVerification(tokenTtlMinutes);

            subscriber.EmailVerification.RefreshToken(tokenTtlMinutes);
        }
    }
}
