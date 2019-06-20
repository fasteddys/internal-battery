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
 * Resource that represents a job application record of a candidate.
 *
 * @property {string} name
 *   Required during application update.
 *
 *   Resource name assigned to an application by the API.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}/applications/{application_id}",
 *   for example,
 *   "projects/api-test-project/tenants/foo/profiles/bar/applications/baz".
 *
 * @property {string} externalId
 *   Required.
 *
 *   Client side application identifier, used to uniquely identify the
 *   recruiter.
 *
 *   The maximum number of allowed characters is 255.
 *
 * @property {string} profile
 *   Required.
 *
 *   Resource name of the candidate of this application.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}",
 *   for example, "projects/api-test-project/tenants/foo/profiles/bar".
 *
 * @property {string} job
 *   One of either a job or a company is required.
 *
 *   Resource name of the job which the candidate applied for.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/jobs/{job_id}",
 *   for example, "projects/api-test-project/tenants/foo/jobs/bar".
 *
 * @property {string} company
 *   One of either a job or a company is required.
 *
 *   Resource name of the company which the candidate applied for.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/companies/{company_id}",
 *   for example, "projects/api-test-project/tenants/foo/companies/bar".
 *
 * @property {string} recruiter
 *   Optional.
 *
 *   The resource name of the person who generated this application. The format
 *   is "projects/{project_id}/tenants/{tenant_id}/recruiters/{recruiter_id}",
 *   for example, "projects/api-test-project/tenants/foo/recruiters/bar".
 *
 * @property {Object} applicationDate
 *   Optional.
 *
 *   The application date.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {string} languageCode
 *   Optional.
 *
 *   The language in which this application information is provided.
 *   This is distinct from the language of the job posting or profile.
 *
 * @property {number} source
 *   Required.
 *
 *   Where did the candidate's application to this job orginate (recruiter
 *   sourcing, job board application, direct website, walk-in, and so on)?
 *
 *   The number should be among the values of [ApplicationSource]{@link google.cloud.talent.v4beta1.ApplicationSource}
 *
 * @property {string} sourceDetails
 *   Optional.
 *
 *   Free text string indicating any additional information about the
 *   application source, (for example, name of job board, vendor) -
 *   typically is from a list of values within a tenant.
 *
 * @property {number} stage
 *   Required.
 *
 *   What is the most recent stage of the application (that is, new, screen,
 *   send cv, hired, finished work)?  This field is intentionally not
 *   comprehensive of every possible status, but instead, represents statuses
 *   that would be used to indicate to the ML models good / bad matches.
 *
 *   The number should be among the values of [ApplicationStage]{@link google.cloud.talent.v4beta1.ApplicationStage}
 *
 * @property {number} state
 *   Optional.
 *
 *   The application state.
 *
 *   The number should be among the values of [ApplicationState]{@link google.cloud.talent.v4beta1.ApplicationState}
 *
 * @property {Object[]} references
 *   Optional.
 *
 *   Details of any reference checks performed on the candidate.
 *
 *   This object should have the same structure as [Reference]{@link google.cloud.talent.v4beta1.Reference}
 *
 * @property {Object[]} assessments
 *   Optional.
 *
 *   Any asessments done on the candidate as part of this application.
 *
 *   This object should have the same structure as [Assessment]{@link google.cloud.talent.v4beta1.Assessment}
 *
 * @property {Object[]} interviewAvailability
 *   Optional.
 *
 *   When the candidate is available for interviewing.
 *
 *   This object should have the same structure as [TimeSegment]{@link google.cloud.talent.v4beta1.TimeSegment}
 *
 * @property {Object[]} interviews
 *   Optional.
 *
 *   All interviews (screen, onsite, and so on) conducted as part of this
 *   application (includes details such as user conducting the interview,
 *   timestamp, feedback, and so on).
 *
 *   This object should have the same structure as [Interview]{@link google.cloud.talent.v4beta1.Interview}
 *
 * @property {Object[]} offers
 *   Optional.
 *
 *   All job offers associated wih this application.
 *
 *   This object should have the same structure as [ApplicationOffer]{@link google.cloud.talent.v4beta1.ApplicationOffer}
 *
 * @property {Object} referral
 *   Optional.
 *
 *   If the candidate is referred by a employee.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {Object} createTime
 *   Required.
 *
 *   Reflects the time that the application was created.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {Object} updateTime
 *   Optional.
 *
 *   The last update timestamp.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {string} outcomeNotes
 *   Optional.
 *
 *   Free text reason behind the recruitement outcome (for example, reason for
 *   withdraw / reject, reason for an unsuccessful finish, and so on).
 *
 *   Number of characters allowed is 100.
 *
 * @property {number} outcome
 *   Optional.
 *
 *   Outcome positiveness shows how positive the outcome is.
 *
 *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
 *
 * @property {Object[]} recruitingNotes
 *   Optional.
 *
 *   Notes from the recruiter about this application that do not fit
 *   into the other structured fields.
 *
 *   This object should have the same structure as [RecruitingNote]{@link google.cloud.talent.v4beta1.RecruitingNote}
 *
 * @property {Object[]} compensationPreferences
 *   Optional.
 *
 *   Any known preferences of the candidate's compensation (salary, bonuses,
 *   and so on).
 *
 *   This object should have the same structure as [CompensationInfo]{@link google.cloud.talent.v4beta1.CompensationInfo}
 *
 * @property {Object} proactiveSubmission
 *   Optional.
 *
 *   Information about if this application is a pro-active submission.
 *
 *   This object should have the same structure as [ProactiveSubmission]{@link google.cloud.talent.v4beta1.ProactiveSubmission}
 *
 * @property {Object} isSupervisor
 *   Optional.
 *
 *   If it is a supervised position.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {number} supervisedEmployeeCount
 *   Optional.
 *
 *   The number of employees this person has supervised.
 *
 * @property {Object} isMatch
 *   Output only. Indicates whether this job application is a match to
 *   application related filters. This value is only applicable in profile
 *   search response.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {string} jobTitleSnippet
 *   Output only. Job title snippet shows how the job title is related to a
 *   search query. It's empty if the job title isn't related to the search
 *   query.
 *
 * @typedef Application
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Application definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/application.proto}
 */
const Application = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * The information representing if this application is a pro-active
   * submission.
   *
   * @property {boolean} proactive
   *   Optional.
   *
   *   Set to true if this application represents a pro-active submission.
   *   Default to false.
   *
   * @property {string} notes
   *   Optional.
   *
   *   Additional supporting information if this application is a pro-active
   *   submittal, such as possible role fits, or a potential hiring manager.
   *
   * @typedef ProactiveSubmission
   * @memberof google.cloud.talent.v4beta1
   * @see [google.cloud.talent.v4beta1.Application.ProactiveSubmission definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/application.proto}
   */
  ProactiveSubmission: {
    // This is for documentation. Actual contents will be loaded by gRPC.
  },

  /**
   * Where did the candidate's application to this job orginate (recruiter
   * sourcing, job board application, direct website, walk-in, and so on)?
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  ApplicationSource: {
    APPLICATION_SOURCE_UNSPECIFIED: 0,

    /**
     * Recruiter sourced the candidate (whether from internal database, their
     * network, and so on).
     */
    RECRUITER_SOURCED: 1,

    /**
     * Candidate applied to the job through a desktop web browser.
     */
    APPLY_DIRECT_WEB: 2,

    /**
     * Candidate applied to the job through a mobile web browser.
     */
    APPLY_DIRECT_MOBILE_WEB: 3,

    /**
     * Candidate applied to the job through a mobile app.
     */
    APPLY_DIRECT_MOBILE_APP: 4,

    /**
     * Candidate applied to the job through a walk in / in person.
     */
    APPLY_DIRECT_IN_PERSON: 5,

    /**
     * Candidate applies to the job through a 3rd party job board or aggregator.
     */
    APPLY_INDIRECT: 6,

    /**
     * Candidate was referred to the job.
     */
    REFERRAL: 7,

    /**
     * Another candidate source not specified.
     */
    OTHER_APPLICATION_SOURCE: 8
  },

  /**
   * The stage of the application.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  ApplicationStage: {

    /**
     * Default value.
     */
    APPLICATION_STAGE_UNSPECIFIED: 0,

    /**
     * Candidate has applied or a recruiter put candidate into consideration but
     * candidate is not yet screened / no decision has been made to move or not
     * move the candidate to the next stage.
     */
    NEW: 1,

    /**
     * A recruiter decided to screen the candidate for this role.
     */
    SCREEN: 2,

    /**
     * Candidate is being / was sent to the customer / hiring manager for
     * detailed review.
     */
    HIRING_MANAGER_REVIEW: 3,

    /**
     * Candidate was approved by the client / hiring manager and is being / was
     * interviewed for the role.
     */
    INTERVIEW: 4,

    /**
     * Candidate will be / has been given an offer of employment.
     */
    OFFER_EXTENDED: 5,

    /**
     * Candidate has accepted their offer of employment.
     */
    OFFER_ACCEPTED: 6,

    /**
     * Candidate has begun (or completed) their employment or assignment with
     * the employer.
     */
    STARTED: 7
  },

  /**
   * Enum that represents the application status.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  ApplicationState: {

    /**
     * Default value.
     */
    APPLICATION_STATE_UNSPECIFIED: 0,

    /**
     * The current stage is in progress or pending, for example, interviews in
     * progress.
     */
    IN_PROGRESS: 1,

    /**
     * The current stage was terminated by a candidate decision.
     */
    CANDIDATE_WITHDREW: 2,

    /**
     * The current stage was terminated by an employer or agency decision.
     */
    EMPLOYER_WITHDREW: 3,

    /**
     * The current stage is successfully completed, but the next stage (if
     * applicable) has not begun.
     */
    COMPLETED: 4,

    /**
     * The current stage was closed without an exception, or terminated for
     * reasons unrealated to the candidate.
     */
    CLOSED: 5
  }
};