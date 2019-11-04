using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Dto.User
{
    public class UserDto
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(32)]
        public string Password { get; set; }
    }
}
