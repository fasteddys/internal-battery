using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IContactService
    {
       Task CreateNewMessage(ContactUsDto contactDto);
       Task CreateHireTalentMessage(HireTalentDto hireTalentDto);
    }
}
