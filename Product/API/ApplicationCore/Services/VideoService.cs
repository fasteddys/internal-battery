using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class VideoService : IVideoService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public VideoService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }
    }
}
