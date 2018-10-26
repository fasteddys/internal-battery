using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class State : BaseModel
    {
        public int StateId { get; set; }
        public Guid? StateGuid { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        public int CountryId { get; set; }
        public virtual Country Country { get; set; }
    }
}
