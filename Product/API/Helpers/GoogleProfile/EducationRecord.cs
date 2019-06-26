using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class EducationRecord
    {
        public Timestamp startDate { get; set; }
        public Timestamp endDate { get; set; }
        string schoolName { get; set; }
        string description { get; set; }


        public EducationRecord(SubscriberEducationHistory subscriberEducationHistory)
        {
            if (subscriberEducationHistory.StartDate != null && subscriberEducationHistory.StartDate.Value != DateTime.MinValue)
                this.startDate = new Timestamp()
                {
                    Seconds = Utils.ToUnixTimeInSeconds(subscriberEducationHistory.StartDate.Value),
                    Nanos = 0
                };
            if (subscriberEducationHistory.EndDate != null && subscriberEducationHistory.EndDate.Value != DateTime.MinValue)
                this.endDate = new Timestamp()
                {
                    Seconds = Utils.ToUnixTimeInSeconds(subscriberEducationHistory.EndDate.Value),
                    Nanos = 0
                };
            this.schoolName = subscriberEducationHistory.EducationalInstitution?.Name;
            this.description = subscriberEducationHistory.EducationalDegree?.Degree + " " + subscriberEducationHistory.EducationalDegreeType?.DegreeType;
        }
    }

}
