using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Gender : BaseModel
    {
        public int GenderId { get; set; }
        public Guid? GenderGuid { get; set; }
        [Required]
        public string SexualIdentification { get; set; }
    }
}
