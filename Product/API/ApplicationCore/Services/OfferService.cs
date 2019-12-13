using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class OfferService : IOfferService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IHangfireService _hangfireService;
        private readonly IMapper _mapper;

        public OfferService(IHangfireService hangfireService, IMapper mapper, IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<List<OfferDto>> GetAllOffers(int limit = 5, int offset = 0)
        {
            var offers = await _repositoryWrapper.Offer.GetAllOffers(limit, offset);
            var offerDto = _mapper.Map<List<OfferDto>>(offers);
            return offerDto;
        }

    }
}
