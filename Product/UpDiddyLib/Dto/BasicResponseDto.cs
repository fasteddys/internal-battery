using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class BasicResponseDto : BaseDto
    {
        private string _StatusCode;

        public string StatusCode
        {
            get { return _StatusCode; }
            set { _StatusCode = value; }
        }

        private string _Description;

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }


    }
}
