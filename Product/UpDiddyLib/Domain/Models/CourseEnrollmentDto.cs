using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class CourseEnrollmentDto
    {

        #region Braintree Stuff 
        private Decimal _PaymentAmount;

        public Decimal PaymentAmount
        {
            get { return _PaymentAmount; }
            set { _PaymentAmount = value; }
        }

        private string _Nonce;

        public string Nonce
        {
            get { return _Nonce; }
            set { _Nonce = value; }
        }

        private string _FirstName;

        public string FirstName
        {
            get { return _FirstName; }
            set { _FirstName = value; }
        }

        private string _LastName;

        public string LastName
        {
            get { return _LastName; }
            set { _LastName = value; }
        }

        private string _PhoneNumber;

        public string PhoneNumber
        {
            get { return _PhoneNumber; }
            set { _PhoneNumber = value; }
        }

        private string _Email;

        public string Email
        {
            get { return _Email; }
            set { _Email = value; }
        }

        private string _Address;

        public string Address
        {
            get { return _Address; }
            set { _Address = value; }
        }

        private string _Region;

        public string Region
        {
            get { return _Region; }
            set { _Region = value; }
        }

        private string _Locality;

        public string Locality
        {
            get { return _Locality; }
            set { _Locality = value; }
        }

        private string _ZipCode;

        public string ZipCode
        {
            get { return _ZipCode; }
            set { _ZipCode = value; }
        }

        private string _CountryCode;

        public string CountryCode
        {
            get { return _CountryCode; }
            set { _CountryCode = value; }
        }

        private string _MerchantAccountId;

        public string MerchantAccountId
        {
            get { return _MerchantAccountId; }
            set { _MerchantAccountId = value; }
        }

        public Guid? StateGuid { get; set; }

        public Guid? CountryGuid { get; set; }

        #endregion



    }

}
  
