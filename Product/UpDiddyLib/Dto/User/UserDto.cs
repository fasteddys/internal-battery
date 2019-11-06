using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Dto.User
{
    public class UserDto : EmailDto
    {
        [Required]
        [StringLength(32)]
        public string Password { get; set; }
    }
}
