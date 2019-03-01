using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CampaignCourseVariantFactory
    {
        public static CampaignCourseVariant CreateCampaignCourseVariant(int CampaignId, int CourseVariantId, int MaxRebateEligibilityInDays,
               bool IsEligibleForRebate, int RebateTypeId, int? RefundId)
        {
            return new CampaignCourseVariant()
            {
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                IsDeleted = 0,
                CreateGuid = Guid.Empty,
                ModifyGuid = Guid.Empty,
                CampaignCourseVariantGuid = Guid.NewGuid(),
                CampaignId = CampaignId,
                CourseVariantId = CourseVariantId,
                MaxRebateEligibilityInDays = MaxRebateEligibilityInDays,
                IsEligibleForRebate = IsEligibleForRebate,
                RebateTypeId = RebateTypeId,
                RefundId = RefundId != null ? RefundId.Value : RefundId
            };




        }
    }
}
