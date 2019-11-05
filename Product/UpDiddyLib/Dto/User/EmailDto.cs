using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Dto.User
{
    public class EmailDto
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }
    }
}
