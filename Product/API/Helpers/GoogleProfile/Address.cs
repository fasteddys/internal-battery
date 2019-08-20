using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class Address
    {
        public Address() { }

        public string unstructuredAddress { get; set; }
        public PostalAddress structuredAddress { get; set; }

        public Address(Subscriber subscriber)
        {
            this.unstructuredAddress = subscriber.Address + " " + subscriber.City + " , " + subscriber.State?.Code + " " + subscriber.PostalCode;
            this.structuredAddress = new PostalAddress()
            {
                revision = 0,
                regionCode = subscriber.State?.Country?.Code2,  // todo change when we go international 
                administrativeArea = subscriber.State?.Code,
                postalCode = subscriber.PostalCode,
                locality = subscriber.City

            };

            if (string.IsNullOrEmpty(this.structuredAddress.regionCode))
                this.structuredAddress.regionCode = Constants.RegionCodeUS;
 
           if ( ! string .IsNullOrEmpty(subscriber.Address))
           {
                this.structuredAddress.addressLines = new List<string>();
                this.structuredAddress.addressLines.Add(subscriber.Address);
           }
            
        }

    }
}
