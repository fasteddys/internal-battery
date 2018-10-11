using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyLib.Dto
{
    public class TopicDto : BaseDto
    {
        private int _TopicId;
        private Guid? _TopicGuid;
        private string _Name;
        private string _Description;
        private string _DesktopImage;
        private string _TabletImage;
        private string _MobileImage;
        private int? _SortOrder;
        private string _Slug;
        private int? _Hidden;

        public int TopicId
        {
            get { return _TopicId; }
            set { _TopicId = value; }
        }

        public Guid? TopicGuid
        {
            get { return _TopicGuid; }
            set { _TopicGuid = value; }
        }

        public string Name
        {
            get { return _Name = _Name ?? ""; }
            set { _Name = value; }
        }

        public string Description
        {
            get { return _Description = _Description ?? ""; }
            set { _Description = value; }
        }

        public string DesktopImage
        {
            get { return _DesktopImage = _DesktopImage ?? ""; }
            set { _DesktopImage = value; }
        }

        public string TabletImage
        {
            get { return _TabletImage = _TabletImage ?? ""; }
            set { _TabletImage = value; }
        }
        public string MobileImage
        {
            get { return _MobileImage = _MobileImage ?? ""; }
            set { _MobileImage = value; }
        }

        public int? SortOrder
        {
            get { return _SortOrder = _SortOrder ?? 0; }
            set { _SortOrder = value; }
        }

        public string Slug
        {
            get { return _Slug = _Slug ?? ""; }
            set { _Slug = value; }
        }

        public int? Hidden
        {
            get { return _Hidden = _Hidden ?? 0; }
            set { _Hidden = value; }
        }
        

    }
}
