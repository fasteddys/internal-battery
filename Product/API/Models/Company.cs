using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Company : BaseModel
    {
        public int CompanyId { get; set; }
        public Guid CompanyGuid { get; set; }
        public string CompanyName { get; set; }
        /// <summary>
        /// The uri returned from google talent cloud for identifying the company
        /// </summary>
        public string GoogleCloudUri { get; set; }
    }
}
