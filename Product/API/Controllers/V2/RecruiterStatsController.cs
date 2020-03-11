using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;

using Microsoft.EntityFrameworkCore;
using UpDiddyLib.Domain.Models.Reports;
using UpDiddyApi.ApplicationCore.Exceptions;
using System.Data.SqlClient;
using AutoMapper;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/recruiter-stats")]
    [ApiController]
    public class RecruiterStatsController : ControllerBase
    {
        private readonly UpDiddyDbContext _upDiddyDbContext;
        private readonly IMapper _mapper;

        public RecruiterStatsController(UpDiddyDbContext upDiddyDbContext, IMapper mapper)
        {
            _upDiddyDbContext = upDiddyDbContext;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> GetRecruiterStats(int year)
        {
            if (year == 0)
                throw new FailedValidationException("year is required");
            var spParams = new object[] { new SqlParameter("@Year", year) };
            List<RecruiterStatDto> recruiterStats = await _upDiddyDbContext.RecruiterStats.FromSql<RecruiterStatDto>("EXEC [dbo].[System_Get_RecruiterStats] @Year", spParams).ToListAsync();
            return Ok(_mapper.Map<RecruiterStatListDto>(recruiterStats));
        }

        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("{recruiterStatId:int}")]
        public async Task<IActionResult> UpdateRecruiterStat(int recruiterStatId, [FromBody] RecruiterStatDto recruiterStatDto)
        {
            var recruiterStat = await _upDiddyDbContext.RecruiterStat.Where(rs => rs.RecruiterStatId == recruiterStatId).FirstOrDefaultAsync();
            if (recruiterStat == null)
                throw new NotFoundException("recruiter stat not found");
            recruiterStat.CCInterviews = recruiterStatDto.CCInterviews;
            recruiterStat.CCSpread = recruiterStatDto.CCSpread;
            recruiterStat.CCStarts = recruiterStatDto.CCStarts;
            recruiterStat.CCSubmittals = recruiterStatDto.CCSubmittals;
            recruiterStat.OpCoInterviews = recruiterStatDto.OpCoInterviews;
            recruiterStat.OpCoSpread = recruiterStatDto.OpCoSpread;
            recruiterStat.OpCoStarts = recruiterStatDto.OpCoStarts;
            recruiterStat.OpCoSubmittals = recruiterStatDto.OpCoSubmittals;
            _upDiddyDbContext.Update(recruiterStat);
            await _upDiddyDbContext.SaveChangesAsync();
            return StatusCode(204);
        }
    }
}