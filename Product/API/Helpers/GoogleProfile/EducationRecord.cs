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

        public EducationRecord() { }

        public Date startDate { get; set; }
        public Date endDate { get; set; }
        public string schoolName { get; set; }
        public string description { get; set; }

        public Degree structuredDegree { get; set; }


        public EducationRecord(SubscriberEducationHistory subscriberEducationHistory)
        {
            if (Utils.validStartDate(subscriberEducationHistory.StartDate, subscriberEducationHistory.EndDate))
                this.startDate = new Date()
                {
                    day = subscriberEducationHistory.StartDate.Value.Day,
                    month = subscriberEducationHistory.StartDate.Value.Month,
                    year = subscriberEducationHistory.StartDate.Value.Year
                };
            if (Utils.validEndDate(subscriberEducationHistory.StartDate, subscriberEducationHistory.EndDate))
                this.endDate = new Date()
                {
                    day = subscriberEducationHistory.EndDate.Value.Day,
                    month = subscriberEducationHistory.EndDate.Value.Month,
                    year = subscriberEducationHistory.EndDate.Value.Year
                };
            this.schoolName = subscriberEducationHistory.EducationalInstitution?.Name;
            this.description = subscriberEducationHistory.EducationalDegree?.Degree + " " + subscriberEducationHistory.EducationalDegreeType?.DegreeType;
        
            this.structuredDegree = new Degree()
            {
                 degreeName = subscriberEducationHistory.EducationalDegreeType?.DegreeType
            };
            this.structuredDegree.fieldsOfStudy = new List<string>();
            this.structuredDegree.fieldsOfStudy.Add(subscriberEducationHistory.EducationalDegree?.Degree);
          
        }
    }

}
