using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class ExperienceLevel : BaseModel
    {
        public int ExperienceLevelId { get; set; }
        public Guid ExperienceLevelGuid { get; set; }
        /// <summary>
        /// The common text asscociated with the experience level. e.g. Entry Level, Experienced, Manager, etc.
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        ///  Alphanumeric code representing the experience level 
        /// </summary>
        public string Code { get; set; }
    }
}
