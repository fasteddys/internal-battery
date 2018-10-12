using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class PromoCodeDto : BaseDto
    {
        private int _PromoCodeId;

        public int PromoCodeId
        {
            get { return _PromoCodeId; }
            set { _PromoCodeId = value; }
        }

        private Guid? _PromoCodeGuid;

        public Guid? PromoCodeGuid
        {
            get { return _PromoCodeGuid; }
            set { _PromoCodeGuid = value; }
        }

        private string _Code;

        public string Code
        {
            get { return _Code; }
            set { _Code = value; }
        }

        private DateTime _PromoStartDate;

        public DateTime PromoStartDate
        {
            get { return _PromoStartDate; }
            set { _PromoStartDate = value; }
        }

        private DateTime _PromoEndDate;

        public DateTime PromoEndDate
        {
            get { return _PromoEndDate; }
            set { _PromoEndDate = value; }
        }

        private int _PromoTypeId;

        public int PromoTypeId
        {
            get { return _PromoTypeId; }
            set { _PromoTypeId = value; }
        }

        private Decimal _PromoValueFactor;

        public Decimal PromoValueFactor
        {
            get { return _PromoValueFactor; }
            set { _PromoValueFactor = value; }
        }

        private string _PromoName;

        public string PromoName
        {
            get { return _PromoName; }
            set { _PromoName = value; }
        }

        private string _PromoDescription;

        public string PromoDescription
        {
            get { return _PromoDescription; }
            set { _PromoDescription = value; }
        }
    }
}
