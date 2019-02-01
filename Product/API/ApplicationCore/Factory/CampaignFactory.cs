using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CampaignFactory
    {
        public static string EnrollmentPromoStatusAsText(EnrollmentDto enrollment)
        {
            string rVal = string.Empty;

            CampaignCourseVariantDto courseInfo = enrollment?.Campaign?.CampaignCourseVariant?.Where(s => s.CourseVariant?.CourseVariantId == enrollment?.CourseId).FirstOrDefault();
            if (courseInfo != null)
            {
                if (courseInfo.RebateType.Name == Constants.CampaignRebate.CampaignRebateType.Employment)
                {
                    if (enrollment.PercentComplete == 100)
                        rVal = Constants.CampaignRebate.Employment_Completed_EligibleMsg;
                    else
                        rVal = Constants.CampaignRebate.Employment_InProgress_EligibleMsg;
                }
                else if (courseInfo.RebateType.Name == Constants.CampaignRebate.CampaignRebateType.CourseCompletion)
                {
                    int promoMaxDays = courseInfo.MaxRebateEligibilityInDays == null ? 0 : (int)courseInfo.MaxRebateEligibilityInDays;
                    if (enrollment.PercentComplete == 100)
                    {
                        bool completedInTime = enrollment.CompletionDate <= enrollment.DateEnrolled.AddDays(promoMaxDays);
                        if (completedInTime)
                            rVal = Constants.CampaignRebate.CourseCompletion_Completed_EligibleMsg;
                        else
                            rVal = Constants.CampaignRebate.CourseCompletion_Completed_NotEligibleMsg;
                    }
                    else
                    {
                        int daysLeft = promoMaxDays - (int)Math.Floor((DateTime.Now - enrollment.DateEnrolled).TotalDays);
                        if (daysLeft > 0)
                            rVal = string.Format(Constants.CampaignRebate.CourseCompletion_InProgress_EligibleMsg, daysLeft);
                        else
                            rVal = Constants.CampaignRebate.CourseCompletion_InProgress_NotEligibleMsg;
                    }
                }
            }
            return rVal;
        }
    }
}
