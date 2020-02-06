using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/contacts/")]
    public class ContactsController : ControllerBase
    {
        private readonly IContactService _contactService;
        public ContactsController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewMessage([FromBody] ContactUsDto contactUsDto)
        {
            await _contactService.CreateNewMessage(contactUsDto);
            return StatusCode(202);
        }

        [HttpPost]
        [Route("hire-talent")]
        public async Task<IActionResult> HireTalentMessage([FromBody] HireTalentDto hireTalentDto)
        {
            await _contactService.CreateHireTalentMessage(hireTalentDto);
            return StatusCode(202);
        }        
    }
}
