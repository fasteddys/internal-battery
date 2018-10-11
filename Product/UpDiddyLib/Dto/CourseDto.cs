using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CourseDto : BaseDto
    {

        private int _CourseId;

        public int CourseId
        {
            get { return _CourseId; }
            set { _CourseId = value; }
        }

        private Guid? _CourseGuid;

        public Guid? CourseGuid
        {
            get { return _CourseGuid; }
            set { _CourseGuid = value; }
        }

        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        private string _Description;

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        private string _Code;

        public string Code
        {
            get { return _Code; }
            set { _Code = value; }
        }
        private Decimal? _Price;

        public Decimal? Price
        {
            get { return _Price; }
            set { _Price = value; }
        }
        private int _TopicId;

        public int TopicId
        {
            get { return _TopicId; }
            set { _TopicId = value; }
        }
        private string _DesktopImage;

        public string DesktopImage
        {
            get { return _DesktopImage; }
            set { _DesktopImage = value; }
        }
        private string _TabletImage;

        public string TabletImage
        {
            get { return _TabletImage; }
            set { _TabletImage = value; }
        }
        private string _MobileImage;

        public string MobileImage
        {
            get { return _MobileImage; }
            set { _MobileImage = value; }
        }

        private int? _VendorId;

        public int? VendorId
        {
            get { return _VendorId; }
            set { _VendorId = value; }
        }

        private int? _SortOrder;

        public int? SortOrder
        {
            get { return _SortOrder; }
            set { _SortOrder = value; }
        }

        private int? _CourseDeliveryId;

        public int? CourseDeliveryId
        {
            get { return _CourseDeliveryId; }
            set { _CourseDeliveryId = value; }
        }

        private string _Slug;

        public string Slug
        {
            get { return _Slug; }
            set { _Slug = value; }
        }

        private int? _Hidden;

        public int? Hidden
        {
            get { return _Hidden; }
            set { _Hidden = value; }
        }

        private string _VideoUrl;

        public string VideoUrl
        {
            get { return _VideoUrl; }
            set { _VideoUrl = value; }
        }


    }
}