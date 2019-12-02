using System.ComponentModel.DataAnnotations;
namespace UpDiddyLib.Domain.Models
{
    public class TraitifyRequestDto
    {
        public string AssessmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", ErrorMessage = "Invalid email address. Please update your supplied email and try again.")]
        public string Email { get; set; }
    }
}