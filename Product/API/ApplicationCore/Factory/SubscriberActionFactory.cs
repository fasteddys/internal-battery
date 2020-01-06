using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
namespace UpDiddyApi.ApplicationCore.Factory
{
    /// <summary>
    /// Originally I created this as an API endpoint since that is what we did for contact tracking. It made sense as an API endpoint there
    /// because some of the actions were occurring outside of the CareerCircle application (e.g. open email). As subscriber actions that we
    /// want to track will occur on our site, it is not necessary to expose this via the API.
    /// </summary>
    public class SubscriberActionFactory : FactoryBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IConfiguration _config = null;
        private readonly ILogger _log = null;
        private readonly IDistributedCache _cache = null;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public SubscriberActionFactory(IRepositoryWrapper repositoryWrapper, UpDiddyDbContext db, IConfiguration configuration, ILogger syslog, IDistributedCache distributedCache)
            : base(db, configuration, syslog, distributedCache)
        {
            _db = db;
            _config = configuration;
            _log = syslog;
            _cache = distributedCache;
            _repositoryWrapper = repositoryWrapper;
        }

        /// <summary>
        /// Tracks each individual occurrence of a subscriber performing a provided action. In addition, it is possible to store an entity which is related to the action - this is optional.
        /// </summary>
        /// <param name="subscriberGuid">The identifier of the subscriber performing the action.</param>
        /// <param name="actionName">The name of the action that the subscriber is performing.</param>
        /// <param name="entityTypeName">The type of entity (e.g. Subscriber, Offer, etc) related to the subscriber's action which is being tracked. This is optional.</param>
        /// <param name="entityGuid">The identifier of the entity related to the subscriber's action being tracked. This is optional.</param>
        /// <returns>True if the subscriber action was valid and tracked successfully; false if something went wrong and it was not tracked and not stored.</returns>
        public bool TrackSubscriberAction(Guid subscriberGuid, string actionName, string entityTypeName = null, Guid? entityGuid = null)
        {
            bool isSuccess = false;

            try
            {
                // load the subscriber
                Subscriber subscriber = _repositoryWrapper.SubscriberRepository.GetAllWithTracking().Where(t => t.IsDeleted == 0 && t.SubscriberGuid == subscriberGuid).FirstOrDefault();
                if (subscriber == null)
                    throw new InvalidOperationException("Subscriber not found");

                // load the action
                var action = _repositoryWrapper.ActionRepository.GetAllWithTracking().Where(a => a.Name == actionName && a.IsDeleted == 0).FirstOrDefault();
                if (action == null)
                    throw new InvalidOperationException("Action not found");

                // load the related entity associated with the action (only if specified)
                EntityType entityType = null;
                int? entityId = null;
                if (!string.IsNullOrWhiteSpace(entityTypeName) && entityGuid.HasValue)
                {
                    // load the entity type
                    entityType = _db.EntityType.Where(et => et.Name == entityTypeName).FirstOrDefault();
                    if (entityType == null)
                        throw new InvalidOperationException("Entity type not found");

                    switch (entityType.Name)
                    {
                        case "Subscriber":
                            var subscriberEntity = _repositoryWrapper.SubscriberRepository.GetAllWithTracking().Where(s => s.SubscriberGuid == entityGuid && s.IsDeleted == 0).FirstOrDefault();
                            if (subscriberEntity == null)
                                throw new InvalidOperationException("Related subscriber entity not found");
                            entityId = subscriberEntity.SubscriberId;
                            break;
                        case "Offer":
                            var offerEntity = _repositoryWrapper.Offer.GetAllWithTracking().Where(o => o.OfferGuid == entityGuid && o.IsDeleted == 0).FirstOrDefault();
                            if (offerEntity == null)
                                throw new InvalidOperationException("Related offer entity not found");
                            entityId = offerEntity.OfferId;
                            break;
                        default:
                            throw new NotSupportedException("Unrecognized entity type");
                    }
                }

                // add the subscriber action to the db
                _repositoryWrapper.SubscriberActionRepository.Create(
                    new SubscriberAction()
                    {
                        SubscriberActionGuid = Guid.NewGuid(),
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        ActionId = action.ActionId,
                        EntityId = entityId,
                        EntityTypeId = entityType == null ? null : (int?)entityType.EntityTypeId,
                        IsDeleted = 0,
                        OccurredDate = DateTime.UtcNow,
                        SubscriberId = subscriber.SubscriberId
                    }).Wait();
                _repositoryWrapper.SaveAsync().Wait();

                // mark as successful if we got to this point
                isSuccess = true;
            }
            catch (Exception e)
            {
                // write to syslog
                _syslog.LogError(e, $"SubscriberActionFactory.RecordSubscriberAction exception: {e.Message}", new object[] { subscriberGuid, actionName, entityTypeName, entityGuid });
            }

            return isSuccess;
        }
    }
}
