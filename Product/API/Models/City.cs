using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class City : BaseModel
    {
        public int CityId { get; set; }
        public Guid? CityGuid { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public int StateId { get; set; }
        public virtual State State { get; set; }
    }
}
