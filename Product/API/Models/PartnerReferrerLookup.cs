using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PartnerReferrer
    {
        public int PartnerReferrerId { get; set; }
        [Column(TypeName = "varchar(3000)")]
        public string Path { get; set; }
        public int PartnerId { get; set; }
    }
}
