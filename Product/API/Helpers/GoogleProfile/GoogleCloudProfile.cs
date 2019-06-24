using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class GoogleCloudProfile
    {

        public string name { get; set; }
        public string external_id { get; set; }
        public DateTime create_time { get; set; } 
        public DateTime update_time { get; set; }
 
        public IList<PersonName> person_names { get; set; } 
        public IList<Address> addresses { get; set; }

        public IList<Skill> skills { get; set; }


        // TODO JAB add skills
        //todo jab add work history 
        // todo jab add education history


    }
}
