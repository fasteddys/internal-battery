using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Services.CourseCrawling.ITProTV
{
    public class ItProTVCategory
    {
        /// There is no way to ascertain the highest level categorization from the course pages because the DOM changes based on click events. 
        /// This could be improved by adding a RawData JSON column to CourseSite and storing this information there so that it can be updated
        /// without a code change if the course site's categorization changes.
        private readonly static Dictionary<string, string> _categoryTopics = new Dictionary<string, string>
        {
            // Category
            {"CompTIA","Category"},
            {"Microsoft","Category"},
            {"Cisco","Category"},
            {"Security Skills","Category"},
            {"Linux","Category"},
            {"(ISC)²","Category"},
            {"EC-Council","Category"},
            {"Apple","Category"},
            {"AWS","Category"},
            {"VMware","Category"},
            {"ITIL®","Category"},
            {"Agile","Category"},
            {"Service Management","Category"},
            {"Project Management","Category"},
            {"PMI®","Category"},
            {"PRINCE2®","Category"},
            {"GIAC","Category"},
            {"ISACA","Category"},
            {"Governance","Category"},
            {"Six Sigma","Category"},
            {"Automation","Category"},
            {"Networking Skills","Category"},
            {"IT Foundation Skills","Category"},
            {"Cloud Technology Skills","Category"},
            {"Virtualization Skills","Category"},
            {"OfficeProTV","Category"},
            {"DevProTV","Category"},
            {"CreativeProTV","Category"},
            {"BizProTV","Category"},
            // Certification
            {"MTA","Certification"},
            {"CCNA","Certification"},
            {"CCNP","Certification"},
            {"CISSP","Certification"},
            {"CCSP","Certification"},
            {"SSCP","Certification"},
            {"MCSA","Certification"},
            {"MCSE","Certification"},
            {"ITIL Master Level","Certification"},
            {"PMP","Certification"},
            {"CEH","Certification"},
            {"CISA","Certification"},
            // Job Role
            {"Developer","Job Role"},
            {"Help Desk/Support","Job Role"},
            {"Business Professional","Job Role"},
            {"Network Admin","Job Role"},
            {"Operations/Project Mgr","Job Role"},
            {"Security Admin","Job Role"},
            {"Sys Admin","Job Role"}
        };

        public ItProTVCategory(string abbreviation, string description, List<string> courseNames)
        {
            if (_categoryTopics.ContainsKey(abbreviation))
                this.Topic = _categoryTopics[abbreviation];
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
