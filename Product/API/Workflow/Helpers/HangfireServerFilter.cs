using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Hangfire.Server;
using Hangfire.Common;
using Serilog;
using Hangfire.Client;
using Hangfire.States;
using Hangfire.Storage;

namespace UpDiddyApi.Workflow.Helpers
{
    public class HangfireServerFilter : JobFilterAttribute,
    IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        public bool IsPreliminaryEnvironment;
        private ILogger _logger;

        public HangfireServerFilter(Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger Logger)
        {
            IsPreliminaryEnvironment = Boolean.Parse(configuration["Environment:IsPreliminary"]);
            _logger = Logger;
        }

        public void OnCreating(CreatingContext context)
        {
            _logger.Information($"Creating a job based on method `{context.Job.Method.Name}`...");
            
        }

        public void OnCreated(CreatedContext context)
        {
            _logger.Information(
                $"Job that is based on method `{context.Job.Method.Name}` has been created with id `{context.BackgroundJob?.Id}`");
        }

        public void OnPerforming(PerformingContext context)
        {
            _logger.Information($"Starting to perform job `{context.BackgroundJob.Id}`");
        }

        public void OnPerformed(PerformedContext context)
        {
            _logger.Information($"Job `{context.BackgroundJob.Id}` has been performed");
        }

        public void OnStateElection(ElectStateContext context)
        {
            var failedState = context.CandidateState as FailedState;
            if (failedState != null)
            {
                _logger.Information(
                    $"Job `{context.BackgroundJob.Id}` has been failed due to an exception `{failedState.Exception}`");
            }
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            _logger.Information(
                $"Job `{context.BackgroundJob.Id}` state was changed from `{context.OldStateName}` to `{context.NewState.Name}`");
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            _logger.Information(
                $"Job `{context.BackgroundJob.Id}` state `{context.OldStateName}` was unapplied.");
        }
    }
}
