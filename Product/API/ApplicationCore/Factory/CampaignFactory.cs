using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CampaignFactory
    {
        public static string EnrollmentPromoStatusAsText(EnrollmentDto enrollment)
        {
            string rVal = string.Empty;

            CampaignCourseVariantDto courseInfo = enrollment?.CampaignCourseVariant;
            if (courseInfo != null)
            {
                if (courseInfo.RebateType.Name == Constants.CampaignRebate.CampaignRebateType.Employment)
                {

                    if (enrollment.PercentComplete == 100)
                        rVal = string.Format(Constants.CampaignRebate.Employment_Completed_EligibleMsg, courseInfo.MaxRebateEligibilityInDays);
                    else
                        rVal = string.Format(Constants.CampaignRebate.Employment_InProgress_EligibleMsg, courseInfo.MaxRebateEligibilityInDays);
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

        public static string OpenOffers(UpDiddyDbContext _db, List<CampaignDto> currentCampaigns, List<EnrollmentDto> enrollments)
        {
            string rVal = string.Empty;
            foreach ( CampaignDto c in currentCampaigns) 
            {    
                foreach ( CampaignCourseVariantDto ccv in c.CampaignCourseVariant )
                {                                    
                    if ( OfferAccepted(ccv,enrollments) == false )
                    {
                        // Create anchor tag to let them navigate to promoted course checkout
                        string courseSlug = CourseVariantFactory.GetCourseVariantCourseSlug(_db, ccv.CourseVariant.CourseVariantId);
                        rVal += c.Description + " <a href='/Course/Checkout/" + courseSlug + "'>click here</a> ";
                    }                    
                }               
            }            
            return rVal;
        }


        public static bool OfferAccepted(CampaignCourseVariantDto campaignCourse, List<EnrollmentDto>  enrollments)
        {
            bool rVal = false;
            foreach ( EnrollmentDto enrollment in enrollments)
            {
                if (enrollment.CourseVariantId == campaignCourse.CourseVariant.CourseVariantId)
                    return true;
            }

            return rVal;
        }

    }
}
