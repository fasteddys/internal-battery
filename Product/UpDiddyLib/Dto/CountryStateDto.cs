using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CountryStateDto : BaseDto
    {
        private string _DisplayName;

        public string DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; }
        }

        private string _Code2;

        public string Code2
        {
            get { return _Code2; }
            set { _Code2 = value; }
        }

        private string _Code3;

        public string Code3
        {
            get { return _Code3; }
            set { _Code3 = value; }
        }

        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _Code;

        public string Code
        {
            get { return _Code; }
            set { _Code = value; }
        }

    }
}
