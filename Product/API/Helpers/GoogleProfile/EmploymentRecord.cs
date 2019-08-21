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

        public EmploymentRecord() { }
        public Date startDate { get; set; }
        public Date endDate { get; set; }
        public string employerName { get; set; }
        public string jobTitle { get; set; }
        public string jobDescription { get; set; }

        public EmploymentRecord(SubscriberWorkHistory subscriberWorkHistory)
        {
             
            
            if ( Utils.validStartDate(subscriberWorkHistory.StartDate, subscriberWorkHistory.EndDate) )
                this.startDate = new Date()
                {
                    day = subscriberWorkHistory.StartDate.Value.Day,
                    month =subscriberWorkHistory.StartDate.Value.Month,
                    year = subscriberWorkHistory.StartDate.Value.Year
                     
                };

            if (Utils.validEndDate(subscriberWorkHistory.StartDate, subscriberWorkHistory.EndDate))
                    this.endDate = new Date()
                {
                    day = subscriberWorkHistory.EndDate.Value.Day,
                    month = subscriberWorkHistory.EndDate.Value.Month,
                    year = subscriberWorkHistory.EndDate.Value.Year
                }; 
            this.employerName = subscriberWorkHistory.Company?.CompanyName;
            this.jobTitle = subscriberWorkHistory.Title;
            this.jobDescription = subscriberWorkHistory.JobDescription;
        }

     

    }

    

}
