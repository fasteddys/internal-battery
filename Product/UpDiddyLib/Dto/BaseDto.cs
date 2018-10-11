using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class BaseDto
    {
       

        private int _IsDeleted;

        public int IsDeleted
        {
            get { return _IsDeleted; }
            set { _IsDeleted = value; }
        }

        private DateTime _CreateDate;

        public DateTime CreateDate
        {
            get { return _CreateDate; }
            set { _CreateDate = value; }
        }

        private DateTime? _ModifyDate;

        public DateTime? ModifyDate
        {
            get { return _ModifyDate; }
            set { _ModifyDate = value; }
        }

        private Guid _CreateGuid;

        public Guid CreateGuid
        {
            get { return _CreateGuid; }
            set { _CreateGuid = value; }
        }

        private Guid? _ModifyGuid;

        public Guid? ModifyGuid
        {
            get { return _ModifyGuid; }
            set { _ModifyGuid = value; }
        }



    }
}
