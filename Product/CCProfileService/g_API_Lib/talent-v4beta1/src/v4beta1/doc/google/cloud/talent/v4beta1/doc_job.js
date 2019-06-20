// Copyright 2019 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Note: this file is purely for documentation. Any contents are not expected
// to be loaded as the JS file.

/**
 * A Job resource represents a job posting (also referred to as a "job listing"
 * or "job requisition"). A job belongs to a Company, which is the hiring
 * entity responsible for the job.
 *
 * @property {string} name
 *   Required during job update.
 *
 *   The resource name for the job. This is generated by the service when a
 *   job is created.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/jobs/{job_id}", for
 *   example, "projects/api-test-project/tenants/foo/jobs/1234".
 *
 *   Tenant id is optional and the default tenant is used if unspecified, for
 *   example, "projects/api-test-project/jobs/1234".
 *
 *   Use of this field in job queries and API calls is preferred over the use of
 *   requisition_id since this value is unique.
 *
 * @property {string} company
 *   Required.
 *
 *   The resource name of the company listing the job.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/companies/{company_id}", for
 *   example, "projects/api-test-project/tenants/foo/companies/bar".
 *
 *   Tenant id is optional and the default tenant is used if unspecified, for
 *   example, "projects/api-test-project/companies/bar".
 *
 * @property {string} requisitionId
 *   Required.
 *
 *   The requisition ID, also referred to as the posting ID, is assigned by the
 *   client to identify a job. This field is intended to be used by clients
 *   for client identification and tracking of postings. A job isn't allowed
 *   to be created if there is another job with the same company,
 *   language_code and requisition_id.
 *
 *   The maximum number of allowed characters is 255.
 *
 * @property {number} requisitionState
 *   Required.
 *
 *   The current state of the job requisition.
 *
 *   The number should be among the values of [RequisitionState]{@link google.cloud.talent.v4beta1.RequisitionState}
 *
 * @property {string} title
 *   Required.
 *
 *   The title of the job, such as "Software Engineer"
 *
 *   The maximum number of allowed characters is 500.
 *
 * @property {string} internalTitle
 *   Optional.
 *
 *   The internal title of the job, such as "Software Engineer Level 5".
 *
 *   The maximum number of allowed characters is 500.
 *
 * @property {string} description
 *   Required.
 *
 *   The description of the job, which typically includes a multi-paragraph
 *   description of the company and related information. Separate fields are
 *   provided on the job object for responsibilities,
 *   qualifications, and other job characteristics. Use of
 *   these separate job fields is recommended.
 *
 *   This field accepts and sanitizes HTML input, and also accepts
 *   bold, italic, ordered list, and unordered list markup tags.
 *
 *   The maximum number of allowed characters is 100,000.
 *
 * @property {string} internalDescription
 *   Optional.
 *
 *   The internal description or statement of work for a job, intended for
 *   details or recruiter notes not intended to be externally known or formally
 *   described.
 *
 * @property {string} externalSystem
 *   Optional.
 *
 *   The system of record / source system / operating company from which this
 *   job is extracted. This is intended to be populated where the same job might
 *   be represented differently to different recruiters.
 *
 *   From a training perspective it is important to maintain a separation of
 *   data that best represents the information an individual recruiter had
 *   access to during the search and placement process.
 *
 * @property {string[]} addresses
 *   Optional but strongly recommended for the best service experience.
 *
 *   Location(s) where the employer is looking to hire for this job posting.
 *
 *   Specifying the full street address(es) of the hiring location enables
 *   better API results, especially job searches by commute time.
 *
 *   At most 50 locations are allowed for best search performance. If a job has
 *   more locations, it is suggested to split it into multiple jobs with unique
 *   requisition_ids (e.g. 'ReqA' becomes 'ReqA-1', 'ReqA-2', and so on.) as
 *   multiple jobs with the same company[], language_code and
 *   requisition_id are not allowed. If the original requisition_id must
 *   be preserved, a custom field should be used for storage. It is also
 *   suggested to group the locations that close to each other in the same job
 *   for better search experience.
 *
 *   The maximum number of allowed characters is 500.
 *
 * @property {Object} applicationInfo
 *   Optional.
 *
 *   Job application information.
 *
 *   This object should have the same structure as [ApplicationInfo]{@link google.cloud.talent.v4beta1.ApplicationInfo}
 *
 * @property {string[]} recruiters
 *   Optional.
 *
 *   The resource names of the recruiters who are assigned to place for this
 *   job, such as "projects/api-test-project/tenants/foo/recruiters/bar".
 *
 * @property {Object[]} recruitingNotes
 *   Optional.
 *
 *   Recruiter's free-text internal notes on the job including notes on skill
 *   requirements, years of experience requirements, and so on.
 *
 *   This object should have the same structure as [RecruitingNote]{@link google.cloud.talent.v4beta1.RecruitingNote}
 *
 * @property {string} jobAuthor
 *   Optional.
 *
 *   An identifier for the author or creator of the job, the individual (e.g.
 *   recruiter or account manager) who wrote the job description(s).
 *
 * @property {string} hiringManager
 *   Optional.
 *
 *   An identifier for the hiring manager for this job, the individual who
 *   ultimately makes the decision on who to hire for the position.
 *
 * @property {number[]} jobBenefits
 *   Optional.
 *
 *   The benefits included with the job.
 *
 *   The number should be among the values of [JobBenefit]{@link google.cloud.talent.v4beta1.JobBenefit}
 *
 * @property {Object} compensationInfo
 *   Optional.
 *
 *   Job compensation information (a.k.a. "pay rate") i.e., the compensation
 *   that will paid to the employee.
 *
 *   This object should have the same structure as [CompensationInfo]{@link google.cloud.talent.v4beta1.CompensationInfo}
 *
 * @property {Object} billRate
 *   Optional.
 *
 *   Job compensation information for the "bill rate" i.e., the amount that will
 *   be billed to the employer.
 *
 *   This object should have the same structure as [CompensationInfo]{@link google.cloud.talent.v4beta1.CompensationInfo}
 *
 * @property {Object.<string, Object>} customAttributes
 *   Optional.
 *
 *   A map of fields to hold both filterable and non-filterable custom job
 *   attributes that are not covered by the provided structured fields.
 *
 *   The keys of the map are strings up to 64 bytes and must match the
 *   pattern: [a-zA-Z][a-zA-Z0-9_]*. For example, key0LikeThis or
 *   KEY_1_LIKE_THIS.
 *
 *   At most 100 filterable and at most 100 unfilterable keys are supported.
 *   For filterable `string_values`, across all keys at most 200 values are
 *   allowed, with each string no more than 255 characters. For unfilterable
 *   `string_values`, the maximum total size of `string_values` across all keys
 *   is 50KB.
 *
 * @property {number[]} degreeTypes
 *   Optional.
 *
 *   The desired education degrees for the job, such as Bachelors, Masters.
 *
 *   The number should be among the values of [DegreeType]{@link google.cloud.talent.v4beta1.DegreeType}
 *
 * @property {string} department
 *   Optional.
 *
 *   The department or functional area within the company with the open
 *   position.
 *
 *   The maximum number of allowed characters is 255.
 *
 * @property {number[]} employmentTypes
 *   Optional.
 *
 *   The employment type(s) of a job, for example,
 *   full time or
 *   part time.
 *
 *   The number should be among the values of [EmploymentType]{@link google.cloud.talent.v4beta1.EmploymentType}
 *
 * @property {string} incentives
 *   Optional.
 *
 *   A description of bonus, commission, and other compensation
 *   incentives associated with the job not including salary or pay.
 *
 *   The maximum number of allowed characters is 10,000.
 *
 * @property {string} languageCode
 *   Optional.
 *
 *   The language of the posting. This field is distinct from
 *   any requirements for fluency that are associated with the job.
 *
 *   Language codes must be in BCP-47 format, such as "en-US" or "sr-Latn".
 *   For more information, see
 *   [Tags for Identifying Languages](https://tools.ietf.org/html/bcp47){:
 *   class="external" target="_blank" }.
 *
 *   If this field is unspecified and Job.description is present, detected
 *   language code based on Job.description is assigned, otherwise
 *   defaults to 'en_US'.
 *
 * @property {string[]} classifications
 *   Optional.
 *
 *   The internal classification for this job, often a category, code, or
 *   concatenated values.
 *
 * @property {number} jobLevel
 *   Optional.
 *
 *   The experience level associated with the job, such as "Entry Level".
 *
 *   The number should be among the values of [JobLevel]{@link google.cloud.talent.v4beta1.JobLevel}
 *
 * @property {number} promotionValue
 *   Optional.
 *
 *   A promotion value of the job, as determined by the client.
 *   The value determines the sort order of the jobs returned when searching for
 *   jobs using the featured jobs search call, with higher promotional values
 *   being returned first and ties being resolved by relevance sort. Only the
 *   jobs with a promotionValue >0 are returned in a FEATURED_JOB_SEARCH.
 *
 *   Default value is 0, and negative values are treated as 0.
 *
 * @property {string} qualifications
 *   Optional.
 *
 *   A description of the qualifications required to perform the
 *   job. The use of this field is recommended
 *   as an alternative to using the more general description field.
 *
 *   This field accepts and sanitizes HTML input, and also accepts
 *   bold, italic, ordered list, and unordered list markup tags.
 *
 *   The maximum number of allowed characters is 10,000.
 *
 * @property {string} responsibilities
 *   Optional.
 *
 *   A description of job responsibilities. The use of this field is
 *   recommended as an alternative to using the more general description
 *   field.
 *
 *   This field accepts and sanitizes HTML input, and also accepts
 *   bold, italic, ordered list, and unordered list markup tags.
 *
 *   The maximum number of allowed characters is 10,000.
 *
 * @property {string[]} workEnvironments
 *   Optional.
 *
 *   The work environment in which the person placed in this job will be
 *   operating, for instance in a warehouse or factory.
 *
 * @property {Object[]} skills
 *   Optional.
 *
 *   The skills required for a given job.
 *
 *   This object should have the same structure as [Skill]{@link google.cloud.talent.v4beta1.Skill}
 *
 * @property {Object[]} languageFluencies
 *   Optional.
 *
 *   The language skills required for a given job.
 *
 *   This object should have the same structure as [LanguageFluency]{@link google.cloud.talent.v4beta1.LanguageFluency}
 *
 * @property {Object[]} certifications
 *   Optional.
 *
 *   The certifications required for a given job.
 *
 *   This object should have the same structure as [Certification]{@link google.cloud.talent.v4beta1.Certification}
 *
 * @property {Object[]} workHours
 *   Optional.
 *
 *   The hours / dates required for this job.
 *
 *   This object should have the same structure as [TimeSegment]{@link google.cloud.talent.v4beta1.TimeSegment}
 *
 * @property {number} postingRegion
 *   Optional.
 *
 *   The job PostingRegion (for example, state, country) throughout
 *   which the job is available. If this field is set, a LocationFilter
 *   in a search query within the job region finds this job posting if an
 *   exact location match isn't specified. If this field is set to
 *   PostingRegion.NATION or PostingRegion.ADMINISTRATIVE_AREA,
 *   setting job Job.addresses to the same location level as this field
 *   is strongly recommended.
 *
 *   The number should be among the values of [PostingRegion]{@link google.cloud.talent.v4beta1.PostingRegion}
 *
 * @property {string[]} securityClearances
 *   Optional.
 *
 *   If the job requires a security clearance, what type(s)?
 *
 * @property {string} aggregatorName
 *   Optional.
 *
 *   If the job requisition is being filled through a VMS or MSP, the name of
 *   the aggregator.
 *
 * @property {number} visibility
 *   Optional.
 *
 *   The visibility of the job.
 *
 *   Defaults to Visibility.ACCOUNT_ONLY if not specified.
 *
 *   The number should be among the values of [Visibility]{@link google.cloud.talent.v4beta1.Visibility}
 *
 * @property {Object} jobStartTime
 *   Optional.
 *
 *   The start timestamp of the job in UTC time zone. Typically this field
 *   is used for contracting engagements. Invalid timestamps are ignored.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {Object} jobEndTime
 *   Optional.
 *
 *   The end timestamp of the job. Typically this field is used for contracting
 *   engagements. Invalid timestamps are ignored.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {number} expectedAssignmentDays
 *   Optional.
 *
 *   The expected number of working days this assignment will be (working days
 *   refers to the days the employee will work not to business days). If this
 *   value is zero, this value is ignored and this job has no end date.
 *
 * @property {Object} postingPublishTime
 *   Optional.
 *
 *   The timestamp this job posting was most recently published. The default
 *   value is the time the request arrives at the server. Invalid timestamps are
 *   ignored.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {Object} postingExpireTime
 *   Optional but strongly recommended for the best service
 *   experience.
 *
 *   The expiration timestamp of the job. After this timestamp, the
 *   job is marked as expired, and it no longer appears in search results. The
 *   expired job can't be deleted or listed by the DeleteJob and
 *   ListJobs APIs, but it can be retrieved with the GetJob API or
 *   updated with the UpdateJob API. An expired job can be updated and
 *   opened again by using a future expiration timestamp. Updating an expired
 *   job fails if there is another existing open job with same
 *   company[], language_code and requisition_id.
 *
 *   The expired jobs are retained in our system for 90 days. However, the
 *   overall expired job count cannot exceed 3 times the maximum of open jobs
 *   count over the past week, otherwise jobs with earlier expire time are
 *   cleaned first. Expired jobs are no longer accessible after they are cleaned
 *   out.
 *
 *   Invalid timestamps are ignored, and treated as expire time not provided.
 *
 *   Timestamp before the instant request is made is considered valid, the job
 *   will be treated as expired immediately.
 *
 *   If this value isn't provided at the time of job creation or is invalid,
 *   the job posting expires after 30 days from the job's creation time. For
 *   example, if the job was created on 2017/01/01 13:00AM UTC with an
 *   unspecified expiration date, the job expires after 2017/01/31 13:00AM UTC.
 *
 *   If this value isn't provided on job update, it depends on the field masks
 *   set by UpdateJobRequest.update_mask. If the field masks include
 *   expiry_time, or the masks are empty meaning that every field is
 *   updated, the job posting expires after 30 days from the job's last
 *   update time. Otherwise the expiration date isn't updated.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {Object} postingCreateTime
 *   Output only. The timestamp when this job posting was created.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {Object} postingUpdateTime
 *   Output only. The timestamp when this job posting was last updated.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {string} companyDisplayName
 *   Output only. Display name of the company listing the job.
 *
 * @property {Object} derivedInfo
 *   Output only. Derived details about the job posting.
 *
 *   This object should have the same structure as [DerivedInfo]{@link google.cloud.talent.v4beta1.DerivedInfo}
 *
 * @property {Object} processingOptions
 *   Optional.
 *
 *   Options for job processing.
 *
 *   This object should have the same structure as [ProcessingOptions]{@link google.cloud.talent.v4beta1.ProcessingOptions}
 *
 * @typedef Job
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Job definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/job.proto}
 */
const Job = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * Application related details of a job posting.
   *
   * @property {string[]} emails
   *   Optional.
   *
   *   Use this field to specify email address(es) to which resumes or
   *   applications can be sent.
   *
   *   The maximum number of allowed characters for each entry is 255.
   *
   * @property {string} instruction
   *   Optional.
   *
   *   Use this field to provide instructions, such as "Mail your application
   *   to ...", that a candidate can follow to apply for the job.
   *
   *   This field accepts and sanitizes HTML input, and also accepts
   *   bold, italic, ordered list, and unordered list markup tags.
   *
   *   The maximum number of allowed characters is 3,000.
   *
   * @property {string[]} uris
   *   Optional.
   *
   *   Use this URI field to direct an applicant to a website, for example to
   *   link to an online application form.
   *
   *   The maximum number of allowed characters for each entry is 2,000.
   *
   * @typedef ApplicationInfo
   * @memberof google.cloud.talent.v4beta1
   * @see [google.cloud.talent.v4beta1.Job.ApplicationInfo definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/job.proto}
   */
  ApplicationInfo: {
    // This is for documentation. Actual contents will be loaded by gRPC.
  },

  /**
   * Output only.
   *
   * Derived details about the job posting.
   *
   * @property {Object[]} locations
   *   Structured locations of the job, resolved from Job.addresses.
   *
   *   locations are exactly matched to Job.addresses in the same
   *   order.
   *
   *   This object should have the same structure as [Location]{@link google.cloud.talent.v4beta1.Location}
   *
   * @property {number[]} jobCategories
   *   Job categories derived from Job.title and Job.description.
   *
   *   The number should be among the values of [JobCategory]{@link google.cloud.talent.v4beta1.JobCategory}
   *
   * @typedef DerivedInfo
   * @memberof google.cloud.talent.v4beta1
   * @see [google.cloud.talent.v4beta1.Job.DerivedInfo definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/job.proto}
   */
  DerivedInfo: {
    // This is for documentation. Actual contents will be loaded by gRPC.
  },

  /**
   * Input only.
   *
   * Options for job processing.
   *
   * @property {boolean} disableStreetAddressResolution
   *   Optional.
   *
   *   If set to `true`, the service does not attempt to resolve a
   *   more precise address for the job.
   *
   * @property {number} htmlSanitization
   *   Optional.
   *
   *   Option for job HTML content sanitization. Applied fields are:
   *
   *   * description
   *   * applicationInfo.instruction
   *   * incentives
   *   * qualifications
   *   * responsibilities
   *
   *   HTML tags in these fields may be stripped if sanitiazation isn't
   *   disabled.
   *
   *   Defaults to HtmlSanitization.SIMPLE_FORMATTING_ONLY.
   *
   *   The number should be among the values of [HtmlSanitization]{@link google.cloud.talent.v4beta1.HtmlSanitization}
   *
   * @typedef ProcessingOptions
   * @memberof google.cloud.talent.v4beta1
   * @see [google.cloud.talent.v4beta1.Job.ProcessingOptions definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/job.proto}
   */
  ProcessingOptions: {
    // This is for documentation. Actual contents will be loaded by gRPC.
  },

  /**
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  RequisitionState: {

    /**
     * The requisition state is unspecified.
     */
    REQUISITION_STATE_UNSPECIFIED: 0,

    /**
     * The requisition is in progress / pending.
     */
    ACTIVE: 1,

    /**
     * Candidate was placed for this requisition.
     */
    FILLED: 2,

    /**
     * Requisition was filled, but lost to a competitor.
     */
    LOST: 3,

    /**
     * Requisition was closed without being filled by Customer or a competitor
     * (e.g., position went away, recruiter changed their mind, and so on).
     */
    CLOSED: 4
  }
};