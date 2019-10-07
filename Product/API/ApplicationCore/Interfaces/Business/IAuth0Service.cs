using System.Threading.Tasks;
using UpDiddyLib.Dto.Marketing;
using Auth0.ManagementApi.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IAuth0Service
    {
            Task<User> CreateUser(SignUpDto signUpDto);

    }
}
