using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class Country : BaseModel
    {
        public int CountryId { get; set; }
        public Guid? CountryGuid { get; set; }
        [Required]
        public string Code2 { get; set; }
        [Required]
        public string Code3 { get; set; }
        public string OfficialName { get; set; }
        [Required]
        public string DisplayName { get; set; }
        public int Sequence { get; set; }
    }
}
