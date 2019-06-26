using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class EmploymentRecord
    {
        public Timestamp startDate { get; set; }
        public Timestamp endDate { get; set; }
        public string employerName { get; set; }
        public string jobTitle { get; set; }
        public string jobDescription { get; set; }

        public EmploymentRecord(SubscriberWorkHistory subscriberWorkHistory)
        {
            if (subscriberWorkHistory.StartDate != null && subscriberWorkHistory.StartDate.Value != DateTime.MinValue)
                this.startDate = new Timestamp()
                {
                    Seconds = Utils.ToUnixTimeInSeconds(subscriberWorkHistory.StartDate.Value),
                    Nanos = 0

                };
            if (subscriberWorkHistory.EndDate != null && subscriberWorkHistory.EndDate.Value != DateTime.MinValue)
                this.endDate = new Timestamp()
                {
                    Seconds = Utils.ToUnixTimeInSeconds(subscriberWorkHistory.EndDate.Value),
                    Nanos = 0
                }; 
            this.employerName = subscriberWorkHistory.Company?.CompanyName;
            this.jobTitle = subscriberWorkHistory.Title;
            this.jobDescription = subscriberWorkHistory.JobDescription;
        }

    }
}
