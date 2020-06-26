using System;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public class Language : BaseModel
    {
        public int LanguageId { get; set; }

        public Guid LanguageGuid { get; set; }

        [Required]
        [StringLength(500)]
        public string LanguageName { get; set; }
    }
}
