using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class Date
    {
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }


        public DateTime ToDate()
        {
            return new DateTime(this.year, this.month, this.day);
        }
    }
}
