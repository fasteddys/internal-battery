using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class PromoCodeController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public PromoCodeController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
        }


        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult Get()
        {

            IList<PromoCodeDto> rval = null;
            rval = _db.PromoCode
                .Where(t => t.IsDeleted == 0)
                .ProjectTo<PromoCodeDto>()
                .ToList();

            return Ok(rval);

        }

        [HttpGet]
        [Route("api/[controller]/{PromoCode}")]
        public IActionResult GetPromoCode(string PromoCode)
        {
            PromoCode promoCode = _db.PromoCode
                .Where(t => t.IsDeleted == 0 && t.Code == PromoCode)
                .FirstOrDefault();

            if (promoCode == null)
                return NotFound();
            return Ok(_mapper.Map<PromoCodeDto>(promoCode));
            
        }

        

        

    }
}