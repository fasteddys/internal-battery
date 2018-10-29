using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class BraintreeResponseDto : BaseDto
    {
        private Boolean _WasSuccessful;

        public Boolean WasSuccessful
        {
            get { return _WasSuccessful; }
            set { _WasSuccessful = value; }
        }


    }
}
