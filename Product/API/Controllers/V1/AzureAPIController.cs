using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Authorization.APIGateway;

namespace UpDiddyApi.Controllers
{
    public class AzureAPIController : Controller
    {
        // GET: /api/azure-api/me
        [HttpGet("/api/azure-api/me")]
        [Authorize(AuthenticationSchemes = APIGatewayDefaults.AuthenticationScheme)]
        public IActionResult MyAccount()
        {
            string userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string email = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            string name = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            return Json(new { userId, email, name, Message = "Success" });
        }
    }
}
