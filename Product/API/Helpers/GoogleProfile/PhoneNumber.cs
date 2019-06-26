using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class PhoneNumber
    {

        public string number {get; set; }

        public PhoneNumber(Subscriber subscriber)
        {
            this.number = subscriber.PhoneNumber;
        }
    }

}
