using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class EmailTemplate : BaseModel
    {
        public int EmailTemplateId { get; set; }
        public Guid EmailTemplateGuid { get; set; }
        public string SendGridTemplateId{ get; set; }
        public string SendGridSubAccount { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Semicolon delimted string of duples that will be used to send data to the sendgrid email template.   
        /// 
        /// Duple Format:  Object Property:Email Template Variable
        /// Duple Example:    FirstName:FirstName;LastName:LastName;City.Name:City
        /// 
        /// Note: Since initial support will be for the g2.profile class and the objects it directly contains (i.e 1 level deep of object support)
        /// City.Name is valid, whereas City.State.Name is NOT valid 
        ///
        /// </summary>
        public string TemplateParams { get; set; }
    }
}
