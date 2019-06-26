using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class PersonName
    {

        public PersonName ( Subscriber subscriber)
        {
            this.formattedName = subscriber.FirstName + " " + subscriber.LastName;
        }


        public string formattedName { get; set; }
    }
}
