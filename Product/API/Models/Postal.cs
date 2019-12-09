using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Postal : BaseModel
    {
        public int PostalId { get; set; }
        public Guid? PostalGuid { get; set; }
        [Required]
        [MaxLength(10)]
        public string Code { get; set; }
        [Required]
        [Column(TypeName = "decimal(12,9)")]
        public decimal Latitude { get; set; }
        [Required]
        [Column(TypeName = "decimal(12,9)")]
        public decimal Longitude { get; set; }
        public int CityId { get; set; }
        public virtual City City { get; set; }
    }
}
