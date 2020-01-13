using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddyLib.Dto;
using X.PagedList;

namespace UpDiddy.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class UserController : BaseController
    {
        public UserController(IApi api, IConfiguration configuration) : base(api, configuration)
        {
        }

        /*
        [HttpGet("jobs")]
        public async Task<IActionResult> MyJobsAsync(int? page)
        {
            page = page.HasValue ? page.Value : 1;
            var pagingDto = await _Api.GetUserJobsOfInterest(page);
            var list = new StaticPagedList<UpDiddyLib.Dto.User.JobDto>(pagingDto.Results, page.Value, pagingDto.PageSize, pagingDto.Count);
            ViewBag.Jobs = list;
            return View();
        }

        [HttpGet("job-alerts")]
        public async Task<IActionResult> MyJobAlertsAsync(int? page)
        {
            page = page.HasValue ? page.Value : 1;
            int? timeZoneOffset = null;
            int tmp;
            if (int.TryParse(Request.Cookies["timeZoneOffset"], out tmp))
                timeZoneOffset = tmp;
            var pagingDto = await _Api.GetUserJobAlerts(page, timeZoneOffset);
            var list = new StaticPagedList<JobPostingAlertDto>(pagingDto.Results, page.Value, pagingDto.PageSize, pagingDto.Count);
            ViewBag.JobAlerts = list;
            return View();
        }

    */
    }
}