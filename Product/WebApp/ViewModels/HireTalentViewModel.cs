using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels
{
    public class HireTalentViewModel
    {
        public string Header { get; set; }
        public string Content { get; set; }
        public string ContactFormHeader { get; set; }
        public string ContactFormText { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Pipeline { get; set; }
        public string SkillSet { get; set; }
        public string Location { get; set; }
        public string Comments { get; set; }
    }
}
