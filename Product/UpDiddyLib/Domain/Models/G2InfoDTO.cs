using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class G2InfoDto
    {

        [JsonProperty("@search.score")]
        public double SearchScore { get; set; }


        public Guid ProfileGuid { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public Guid SubscriberGuid { get; set; }

        public string AvatarUrl { get; set; }

       public DateTime? CreateDate { get; set; }

       public DateTime? ModifyDate { get; set; }

    }
}
