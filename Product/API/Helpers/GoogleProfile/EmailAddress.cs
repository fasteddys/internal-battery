using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class EmailAddress
    {
        public string emailAddress { get; set; }

        public EmailAddress(Subscriber subscriber)
        {
            this.emailAddress = subscriber.Email;
        }
    }
}
