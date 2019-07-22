using UpDiddyApi.Helpers.GoogleProfile;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IGoogleProfileService
    {
        BasicResponseDto AddProfile(GoogleCloudProfile googleCloudProfile, ref string errorMsg);
        BasicResponseDto DeleteProfile(string profileUri, ref string errorMsg);
        BasicResponseDto GetProfile(string profileUri, ref string errorMsg);
        BasicResponseDto Search(SearchProfilesRequest searchRequest, ref string errorMsg);
        bool UpdateProfile(GoogleCloudProfile googleCloudProfile, ref string errorMsg);
    }
}