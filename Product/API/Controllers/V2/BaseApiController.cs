using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace UpDiddyApi.Controllers
{
    public class BaseApiController : ControllerBase
    {
        public BaseApiController() { }
        public Guid GetSubscriberGuid()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Guid subscriberGuid;
                var objectId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (Guid.TryParse(objectId, out subscriberGuid))
                    return subscriberGuid;
                else
                    return Guid.Empty;
            } else
            {
                return Guid.Empty;
            }
        }
    }
}