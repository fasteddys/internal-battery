using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Helpers.CustomDataAnnotations
{
    public class ADB2CPasswordComplexityRequirementAttribute : ValidationAttribute
    {
        public ADB2CPasswordComplexityRequirementAttribute()
        {

        }

        public override bool IsValid(object value)
        {
            string strValue = value as string;

            if (!string.IsNullOrEmpty(strValue))
                return Utils.IsPasswordPassesADB2CRequirements(strValue);

            return false;
        }
    }

    public class Auth0PasswordComplexityRequirementAttribute : ValidationAttribute
    {
        public Auth0PasswordComplexityRequirementAttribute()
        {

        }

        public override bool IsValid(object value)
        {
            string strValue = value as string;

            if (!string.IsNullOrEmpty(strValue))
                return Utils.IsPasswordPassesAuth0Requirements(strValue);

            return false;
        }
    }
}
