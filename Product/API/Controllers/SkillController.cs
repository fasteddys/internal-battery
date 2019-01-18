using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Business.Graph;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Controllers
{
    public class SkillController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        protected internal ILogger _syslog = null;

        public SkillController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<ProfileController> sysLog)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
        }

        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/get/{entityType}/{entityGuid}")]
        public IActionResult Get(string entityType, Guid entityGuid)
        {
            IList<SkillDto> rval = null;
            switch (entityType)
            {
                case "course":
                    rval = _db.CourseSkill
                        .Include(cs => cs.Skill)
                        .Include(cs => cs.Course)
                        .Where(cs => cs.IsDeleted == 0 && cs.Course.CourseGuid == entityGuid)
                        .Select(cs => cs.Skill)
                        .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                        .ToList();
                    break;
                case "subscriber":
                    rval = _db.SubscriberSkill
                     .Include(ss => ss.Skill)
                     .Include(ss => ss.Subscriber)
                     .Where(ss => ss.IsDeleted == 0 && ss.Subscriber.SubscriberGuid == entityGuid)
                     .Select(ss => ss.Skill)
                     .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                     .ToList();
                    break;
                default:
                    throw new NotImplementedException("This entity type is not supported.");
            }

            return Ok(rval);
        }

        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/update")]
        public IActionResult Update([FromBody] EntitySkillDto entitySkillDto)
        {
            if (entitySkillDto == null || entitySkillDto.EntityGuid == Guid.Empty || string.IsNullOrWhiteSpace(entitySkillDto.EntityType))
                return BadRequest();

            var entityGuid = new SqlParameter("@EntityGuid", entitySkillDto.EntityGuid);
            var entityType = new SqlParameter("@EntityType", entitySkillDto.EntityType);

            DataTable table = new DataTable();
            table.Columns.Add("Guid", typeof(Guid));
            if (entitySkillDto.Skills != null)
            {
                foreach (var skill in entitySkillDto.Skills)
                {
                    table.Rows.Add(skill.SkillGuid);
                }
            }

            var skillGuids = new SqlParameter("@SkillGuids", table);
            skillGuids.SqlDbType = SqlDbType.Structured;
            skillGuids.TypeName = "dbo.GuidList";

            var spParams = new object[] { entityGuid, entityType, skillGuids };

            var rowsAffected = _db.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Update_EntitySkills] 
                    @EntityGuid,
                    @EntityType,
	                @SkillGuids", spParams);

            return Ok();
        }
    }
}