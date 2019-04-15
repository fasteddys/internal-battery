using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using Action = UpDiddyApi.Models.Action;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class PartnerContactActionFactory
    {
        public static PartnerContactAction GetPartnerContactAction(UpDiddyDbContext db, Campaign campaign, PartnerContact partnerContact, Action action, CampaignPhase campaignPhase)
        {       
                return db.PartnerContactAction
               .Where(pca => pca.CampaignId == campaign.CampaignId && pca.PartnerContactId == partnerContact.PartnerContactId && pca.ActionId == action.ActionId && pca.CampaignPhaseId == campaignPhase.CampaignPhaseId)
               .FirstOrDefault();
        }
    }
}
