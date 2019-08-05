using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using System.Threading.Tasks;

namespace UpDiddy.Controllers
{
    [Route("[controller]")]
    public class SalesForceController : BaseController
    {
        private IApi _api;
        public SalesForceController(IApi api)
            : base(api)
        {
            _api = api;
        }

        [HttpGet]
        [Route("/SalesForceSignUp")]
        public ActionResult Index()
        {
            SalesForceSignUpListViewModel model = new SalesForceSignUpListViewModel();
            return View(model);
        }

        [HttpPost]
        [Route("/SalesForceSignUp")]
        public async Task<ActionResult> Index(SalesForceSignUpListViewModel model)
        {
            if (ModelState.IsValid)
            {
                SalesForceSignUpListDto dto = new SalesForceSignUpListDto()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email
                };

                await _api.AddSalesForceSignUpList(dto);
                return View("Success");
            } else
            {
               return View(model);
            }
        }
    }
}