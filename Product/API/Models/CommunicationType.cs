using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CommunicationType : BaseModel
    {
        public int CommunicationTypeId { get; set; }
        public Guid? CommunciationTypeGuid { get; set; }
        public int FrequencyInDays { get; set; }
        [Required]
        public string CommunicationName { get; set; }
        public string CommuncationDescription { get; set; }


    }
}
