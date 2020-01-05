using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IEducationalDegreeTypeService
    {
        Task<EducationalDegreeTypeDto> GetEducationalDegreeType(Guid educationalDegreeTypeGuid);
        Task<List<EducationalDegreeTypeDto>> GetEducationalDegreeTypes(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task<List<EducationalDegreeTypeDto>> GetAllEducationDegreeTypes();
        Task UpdateEducationalDegreeType(Guid educationalDegreeTypeGuid, EducationalDegreeTypeDto educationalDegreeTypeDto);
        Task CreateEducationalDegreeType(EducationalDegreeTypeDto educationalDegreeTypeDto);
        Task DeleteEducationalDegreeType(Guid educationalDegreeTypeGuid);
    }
}
