using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace UpDiddyApi.Models.Views
{
    [NotMapped]
    public class v_CandidateAzureSearch
    {





        public string AvatarUrl { get; set; }
        public Guid? PartnerGuid { get; set; }
        public DateTime? CreateDate { get; set; }
        public Guid SubscriberGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postal { get; set; }
        public string Title { get; set; }
        public double? CurrentRate { get; set; }
        public SqlGeography Location { get; set; }
        public string Skills { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string SubscriberLanguages { get; set; }

        public string CommuteDistance { get; set; }
        public bool? IsWillingToTravel { get; set; }
        public bool? IsFlexibleWorkScheduleRequired { get; set; }
        public string EmploymentTypes { get; set; }
        public string SubscriberTraining { get; set; }
        public string SubscriberEducation { get; set; }

        public string SubscriberTitles { get; set; }
        public string SubscriberWorkHistory { get; set; }

        public int? AzureIndexStatusId { get; set; }
 

    }
}
