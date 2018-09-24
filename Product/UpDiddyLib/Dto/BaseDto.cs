using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class BaseDto
    {
        public int IsDeleted { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public Guid CreateGuid { get; set; }
        public Guid? ModifyGuid { get; set; }
    }
}
