using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class WozTransactionLog : BaseModel
    {
        public int WozTransactionLogId { get; set; }
        public string EndPoint { get; set; }
        public string InputParameters { get; set; }
        public string ResponseJson { get; set; }
        public string WozResponseJson { get; set; }
        public Guid? EnrollmentGuid { get; set; }

    }
}
