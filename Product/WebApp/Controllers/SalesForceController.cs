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
        [Route("/SalesForceWaitList")]
        public ActionResult Index()
        {
            SalesForceWaitListViewModel model = new SalesForceWaitListViewModel();
            return View(model);
        }

        [HttpPost]
        [Route("/SalesForceWaitList")]
        public async Task<ActionResult> Index(SalesForceWaitListViewModel model)
        {
            if (ModelState.IsValid)
            {
                SalesForceWaitListDto dto = new SalesForceWaitListDto()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email
                };

                await _api.AddSalesForceWaitList(dto);
                return View("Success");
            } else
            {
               return View(model);
            }
        }
    }
}