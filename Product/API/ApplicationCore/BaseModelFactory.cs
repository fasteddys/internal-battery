using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore
{
    public class BaseModelFactory
    {
        public static void SetDefaultsForAddNew(BaseModel baseModel)
        {

            baseModel.IsDeleted = 0;
            if (baseModel.CreateDate == DateTime.MaxValue || baseModel.CreateDate == DateTime.MinValue)
                baseModel.CreateDate = DateTime.UtcNow;
            if (baseModel.CreateDate == null ||  baseModel.ModifyDate == DateTime.MaxValue || baseModel.ModifyDate == DateTime.MinValue)
                baseModel.ModifyDate = DateTime.UtcNow;
            Guid GuidOutput = Guid.Empty;
            bool isValid = Guid.TryParse(baseModel.CreateGuid.ToString(), out GuidOutput);
            if (isValid == false)
                baseModel.CreateGuid = Guid.NewGuid();
            if ( baseModel.ModifyGuid == null || Guid.TryParse(baseModel.ModifyGuid.ToString(), out GuidOutput) == false )
                baseModel.ModifyGuid = Guid.NewGuid();

        }
    }
}
