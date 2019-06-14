using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;
using X.PagedList;

namespace UpDiddy.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class UserController : BaseController
    {
        private IApi _api;
        public UserController(IApi api) : base(api)
        {
            _api = api;
        }
        
        [HttpGet("jobs")]
        public async Task<IActionResult> MyJobsAsync(int? page)
        {
            page = page.HasValue ? page.Value : 1;
            var pagingDto = await _api.GetUserJobsOfInterest(page);
            var list = new StaticPagedList<UpDiddyLib.Dto.User.JobDto>(pagingDto.Results, page.Value, pagingDto.PageSize, pagingDto.Count);
            ViewBag.Jobs = list;
            return View();
        }
    }
}