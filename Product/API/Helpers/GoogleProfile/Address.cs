using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class Address
    {
        public string unstructuredAddress { get; set; }

        public Address(Subscriber subscriber)
        {
            this.unstructuredAddress = subscriber.Address + " " + subscriber.City + " , " + subscriber.State?.Code + " " + subscriber.PostalCode;
        }

    }
}
