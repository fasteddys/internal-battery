using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class SkillController : Controller
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

        // implement security after task_100 has been merged to this branch as follows: [Authorize("Policy=IsAdminPolicy")]
        [HttpPut]
        [Authorize]
        [Route("api/[controller]")]
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