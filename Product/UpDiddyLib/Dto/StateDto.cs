using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class StateDto : BaseDto
    {
        public int StateId { get; set; }
        public Guid? StateGuid { get; set; }
        public string Code { get; set; }
        public int CountryId { get; set; }

        private string _Name;

        public string Name
        {
            get { return (_Name == null ? "" : _Name); }
            set { _Name = value; }
        }
        public int Sequence { get; set; }
    }
}
