using Microsoft.EntityFrameworkCore;
using System;
using UpDiddyApi.ApplicationCore.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft;

using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;
using UpDiddyLib.Dto;
using Newtonsoft.Json;
using UpDiddyApi.Models.CrossChq;
using UpDiddyLib.Domain.Models.CrossChq;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CrosschqRepository : UpDiddyRepositoryBase<ReferenceCheck>, ICrosschqRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public CrosschqRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddReferenceCheck(Guid profileGuid, Guid recruiterGuid, ReferenceRequestDto referenceRequest, string referenceCheckRequestId)
        {
            //The IsDeleted flag check is not added in the below queries while retrieving the profile and recruiter entities,
            //based on the assumption that the profile and recruiter validity check was done before this method is called.
            //The sole purpose of these DB calls is to get the entities for adding the relational data.
            var profile = await _dbContext.Profile.FirstOrDefaultAsync(p => p.ProfileGuid == profileGuid);
            if (profile == null) throw new NotFoundException($"profile cannot be null: {profileGuid}");

            var recruiter = await _dbContext.Recruiter.FirstOrDefaultAsync(r => r.RecruiterGuid == recruiterGuid);
            if (recruiter == null) throw new NotFoundException($"recruiter cannot be null: {recruiterGuid}");

            var vendor = await _dbContext.ReferenceCheckVendor.FirstOrDefaultAsync(v => v.Name == "Crosschq");
            if (vendor == null) throw new NotFoundException("vendor cannot be null for Crosschq");


            _dbContext.ReferenceCheck.Add(new ReferenceCheck { 
            
               CreateDate = DateTime.UtcNow,
               CreateGuid = Guid.Empty,
               ReferenceCheckGuid = Guid.NewGuid(),
               Recruiter = recruiter,
               Profile = profile,
               ReferenceCheckVendor = vendor,
               CandidateJobTitle = referenceRequest.JobPosition,
               ReferenceCheckType = referenceRequest.JobRole,
               ReferenceCheckRequestId = referenceCheckRequestId
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task<ReferenceCheck> GetReferenceCheckByRequestId(string requestId)
        {
            if (String.IsNullOrWhiteSpace(requestId)) return null;

            var ReferenceCheck = await _dbContext.ReferenceCheck.Where(rc => rc.ReferenceCheckRequestId.Trim() == requestId.Trim())
            .Include(rc => rc.Profile)
            .Include(rc => rc.Recruiter)
            .Include(rc => rc.ReferenceCheckVendor)
            .Include(rc => rc.ReferenceCheckStatus)
            .Include(rc => rc.CandidateReference)
            .FirstOrDefaultAsync();

            return ReferenceCheck;
        }

        public async Task<List<ReferenceCheck>> GetReferenceCheckByProfileGuid(Guid profileGuid)
            => await _dbContext.ReferenceCheck
                .Include(rc => rc.Profile)
                .Include(rc => rc.Recruiter)
                .Include(rc => rc.ReferenceCheckVendor)
                .Include(rc => rc.ReferenceCheckStatus)
                .Include(rc => rc.CandidateReference)
                .OrderByDescending(rc => rc.CreateDate)
                .Where(rc => rc.Profile.ProfileGuid == profileGuid)
                .ToListAsync();

        public async Task<ReferenceCheckReport> GetReferenceCheckReportPdf(Guid referenceCheckGuid, string reportType)
        {
            if (referenceCheckGuid == Guid.Empty || String.IsNullOrWhiteSpace(reportType)) return null;

            var referenceCheckReport = await _dbContext.ReferenceCheckReport
                                      .Where(rcr => rcr.ReferenceCheck.ReferenceCheckGuid == referenceCheckGuid && rcr.FileType == reportType.Trim())
                                      .Include(rcr => rcr.ReferenceCheck)
                                      .OrderByDescending(rcr => rcr.CreateDate)
                                      .FirstOrDefaultAsync();

            return referenceCheckReport;
        }

        public async Task UpdateReferenceCheck(CrosschqWebhookDto crosschqWebhookDto, string fullReportPdfBase64, string summaryReportPdfBase64)
        {
            var referenceCheck = await GetReferenceCheckByRequestId(crosschqWebhookDto.Id);

            if (referenceCheck != null)
            {
                referenceCheck.ModifyDate = DateTime.UtcNow;
                referenceCheck.ModifyGuid = Guid.NewGuid();
                referenceCheck.ReferenceCheckConcludedDate = !referenceCheck.ReferenceCheckConcludedDate.HasValue && crosschqWebhookDto.Progress == 100 ? 
                                                              DateTime.UtcNow : (DateTime?)null;
                if (!String.IsNullOrWhiteSpace(fullReportPdfBase64))
                {
                    referenceCheck.ReferenceCheckReport.Add(new ReferenceCheckReport { 
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            ReferenceCheckReportGuid = Guid.NewGuid(),
                            FileUrl = crosschqWebhookDto.Report_Full_Pdf,
                            Base64File = fullReportPdfBase64,
                            FileType = "Full"
                    });
                }

                if (!String.IsNullOrWhiteSpace(summaryReportPdfBase64))
                {
                    referenceCheck.ReferenceCheckReport.Add(new ReferenceCheckReport
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        ReferenceCheckReportGuid = Guid.NewGuid(),
                        FileUrl = crosschqWebhookDto.Report_Summary_Pdf,
                        Base64File = summaryReportPdfBase64,
                        FileType = "Summary"
                    });
                }

                //Add a new status for every status update
                referenceCheck.ReferenceCheckStatus.Add(new ReferenceCheckStatus
                {
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    ReferenceCheckStatusGuid = Guid.NewGuid(),
                    VendorJsonResponse = JsonConvert.SerializeObject(crosschqWebhookDto),
                    Status = crosschqWebhookDto.Status,
                    Progress = crosschqWebhookDto.Progress
                });

                if (referenceCheck.CandidateReference == null && crosschqWebhookDto.References != null && crosschqWebhookDto.References.Count > 0)
                {
                    //Add new reference
                    foreach (var reference in crosschqWebhookDto.References)
                    {
                        referenceCheck.CandidateReference.Add(new CandidateReference
                        {
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            CandidateReferenceGuid = Guid.NewGuid(),
                            FirstName = reference.First_Name,
                            LastName = reference.Last_Name,
                            Email = reference.Email,
                            PhoneNumber = reference.Mobile_Phone,
                            Status = reference.Status
                        });
                    }
                }
                else if (referenceCheck.CandidateReference != null && crosschqWebhookDto.References != null && crosschqWebhookDto.References.Count >= referenceCheck.CandidateReference.Count)
                {
                    //update reference status - assuming no new references were added/removed/updated
                    foreach (var reference in crosschqWebhookDto.References)
                    {
                        var candidateReferenceEntity = referenceCheck.CandidateReference.FirstOrDefault(cr => cr.Email.Trim() == reference.Email.Trim());

                        if (candidateReferenceEntity != null)
                        {
                            candidateReferenceEntity.ModifyDate = DateTime.UtcNow;
                            candidateReferenceEntity.ModifyGuid = Guid.Empty;
                            candidateReferenceEntity.Status = reference.Status;
                        }
                        else
                        {
                            referenceCheck.CandidateReference.Add(new CandidateReference
                            {
                                CreateDate = DateTime.UtcNow,
                                CreateGuid = Guid.Empty,
                                CandidateReferenceGuid = Guid.NewGuid(),
                                FirstName = reference.First_Name,
                                LastName = reference.Last_Name,
                                Email = reference.Email,
                                PhoneNumber = reference.Mobile_Phone,
                                Status = reference.Status
                            });
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
