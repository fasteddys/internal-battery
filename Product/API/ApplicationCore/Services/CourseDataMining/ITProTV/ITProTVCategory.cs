using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Services.CourseDataMining.ITProTV
{
    public class ItProTVCategory
    {
        public ItProTVCategory(string topic, string abbreviation, string description, List<string> courseNames)
        {
            this.Topic = topic;
            this.Abbreviation = abbreviation;
            this.Description = description;
            this.CourseNames = courseNames;
        }

        public string Topic { get; }
        public string Abbreviation { get; }
        public string Description { get; }
        [JsonIgnore]
        public List<string> CourseNames { get; }
    }
}
