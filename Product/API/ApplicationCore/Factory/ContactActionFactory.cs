using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using Action = UpDiddyApi.Models.Action;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class ContactActionFactory
    {
        public static ContactAction GetContactAction(UpDiddyDbContext db, Campaign campaign, Contact contact, Action action, CampaignPhase campaignPhase)
        {       
                return db.ContactAction
               .Where(ca => ca.CampaignId == campaign.CampaignId && ca.ContactId == contact.ContactId && ca.ActionId == action.ActionId && ca.CampaignPhaseId == campaignPhase.CampaignPhaseId)
               .FirstOrDefault();
        }


    }
}
