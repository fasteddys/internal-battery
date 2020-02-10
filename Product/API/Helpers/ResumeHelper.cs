using System;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using System.Threading.Tasks;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Helpers
{
    public static class ResumeHelper
    {
        public static async Task<Guid> ImportSubscriberProfileDataAsync(
        ISubscriberService subscriberService
        , IRepositoryWrapper repositoryWrapper
        , ISovrenAPI sovrenApi
        , Subscriber subscriber
        , SubscriberFile resume
        , string base64EncodedString)
        {
            resume.Subscriber = subscriber;
            String parsedDocument = await sovrenApi.SubmitResumeAsync(base64EncodedString);
            await SubscriberProfileStagingStoreFactory.Save(repositoryWrapper, resume.Subscriber, Constants.DataSource.Sovren, Constants.DataFormat.Xml, parsedDocument);
            ResumeParse resumeParse = await _ImportSubscriberResume(repositoryWrapper, subscriberService, resume, parsedDocument);

            // TODO JAB Call HiringSolved Parser here 

            return resumeParse.ResumeParseGuid;
        }

        private static async Task<ResumeParse> _ImportSubscriberResume(IRepositoryWrapper repositoryWrapper, ISubscriberService subscriberService, SubscriberFile resumeFile, string resume)
        {
            // Delete all existing resume parses for user
            await repositoryWrapper.ResumeParseRepository.DeleteAllResumeParseForSubscriber(resumeFile.SubscriberId);

            // Create resume parse object 
            ResumeParse resumeParse = await repositoryWrapper.ResumeParseRepository.CreateResumeParse(resumeFile.SubscriberId, resumeFile.SubscriberFileId);

            // Import resume 
            if (await subscriberService.ImportResume(resumeParse, resume) == true)
                resumeParse.RequiresMerge = 1;
            await repositoryWrapper.ResumeParseRepository.SaveAsync();
            return resumeParse;
        }
    }
}