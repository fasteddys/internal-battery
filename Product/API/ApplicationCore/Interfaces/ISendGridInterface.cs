using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.MessageQueue;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface ISendGridInterface
    {
        HttpResponseMessage AddContacts(IList<EmailContactDto> Contacts, ref string ResponseJson);
        HttpResponseMessage AddContactsToList(string ListId, IList<EmailContactDto> Contacts, ref string ResponseJson);
        HttpResponseMessage CreateContactList(SendGridListDto ListInfo, ref string ResponseJson);
        HttpResponseMessage GetContactList(string ListName, ref string ResponseJson);
        HttpResponseMessage GetContactLists(ref string ResponseJson);
        HttpResponseMessage CreateListAndAddContacts(string ListName, IList<EmailContactDto> Contacts, ref string ResponseJson);
    }
}