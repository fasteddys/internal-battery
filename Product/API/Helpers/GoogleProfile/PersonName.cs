using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class PersonName
    {

        public PersonName()
        {

        }

        public PersonName ( Subscriber subscriber)
        {
            this.formattedName = subscriber.FirstName + " " + subscriber.LastName;
            this.structuredName = new PersonStructuredName()
            {
                familyName = subscriber.LastName,
                givenName = subscriber.FirstName
            };
        }


        public string formattedName { get; set; }
        public PersonStructuredName structuredName { get; set; }
    }
}
