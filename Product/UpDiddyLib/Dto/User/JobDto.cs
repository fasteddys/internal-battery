using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.User
{
    public class JobDto
    {
        public Guid? JobPostingFavoriteGuid { get; set; }
        public Guid? JobPostingGuid { get; set; }
        public string Title { get; set; }
        public string CityProvince { get; set; }
        public string CompanyName { get; set; }
        public DateTime PostingExpirationDateUTC { get; set;  }
        public int? JobApplicationId { get; set; }
        public int? SubscriberId { get; set; }
    }
}