using System;
namespace UpDiddyLib.Domain.Models
{
    public class ContactUsDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactType { get; set; }
        public string Message { get; set; }
    }
}