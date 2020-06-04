using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft;

using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;
using UpDiddyLib.Dto;
using Newtonsoft.Json;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CrosschqRepository : UpDiddyRepositoryBase<ReferenceCheck>, ICrosschqRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public CrosschqRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
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

        public async Task UpdateReferenceCheck(CrosschqWebhookDto crosschqWebhookDto, string fullReportPdfBase64, string summaryReportPdfBase64)
        {
            var referenceCheck = await GetReferenceCheckByRequestId(crosschqWebhookDto.Id);

            if (referenceCheck != null)
            {
                referenceCheck.ModifyDate = DateTime.UtcNow;
                referenceCheck.ModifyGuid = Guid.NewGuid();
                //referenceCheck.ReferenceCheckReportFile = fullReportPdfBase64;
                //referenceCheck.ReferenceCheckReportFileUrl = crosschqWebhookDto.Report_Full_Pdf; //??????

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
