using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class JobApplicationStatus : BaseModel
    {

        public int JobApplicationStatusId { get; set; }

        public Guid JobApplicationStatusGuid { get; set; }

        public string Status { get; set; }




    }
}
