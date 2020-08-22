using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpDiddyApi.Models
{
    [Table("InvalidEmails", Schema = "B2B")]
    public class InvalidEmail : BaseModel
    {
        public int InvalidEmailId { get; set; }

        public Guid InvalidEmailGuid { get; set; }

        [StringLength(500)]
        public string Value { get; set; }
    }
}
