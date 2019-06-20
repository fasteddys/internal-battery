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
 * Resource that represents a job assignment record of a candidate.
 *
 * @property {string} name
 *   Required during assignment update.
 *
 *   Resource name assigned to an assignment by the API.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}/assignments/{assignment_id}",
 *   for example,
 *   "projects/api-test-project/tenants/foo/profiles/bar/assignments/baz".
 *
 * @property {string} externalId
 *   Required.
 *
 *   Client side assignment identifier, used to uniquely identify the
 *   assignment.
 *
 *   The maximum number of allowed characters is 255.
 *
 * @property {string} profile
 *   Required.
 *
 *   Resource name of the profile of this assignment.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}",
 *   for example, "projects/api-test-project/tenants/foo/profiles/bar".
 *
 * @property {string} application
 *   Optional.
 *
 *   Resource name of the application generating this assignment.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}/applications/{application_id}",
 *   for example,
 *   "projects/api-test-project/tenants/foo/profiles/bar/applications/baz".
 *
 * @property {string} job
 *   One of either a job or a company is required.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/jobs/{job_id}",
 *   for example, "projects/api-test-project/tenants/foo/jobs/bar".
 *
 * @property {string} company
 *   One of either a job or a company is required.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/companies/{company_id}",
 *   for example, "projects/api-test-project/tenants/foo/companies/bar".
 *
 * @property {string} recruiter
 *   Optional.
 *
 *   The resource name of the person who generated this assignment. The format
 *   is "projects/{project_id}/tenants/{tenant_id}/recruiters/{recruiter_id}",
 *   for example, "projects/api-test-project/tenants/foo/recruiters/bar".
 *
 * @property {Object} startDate
 *   Optional.
 *
 *   The expected or actual start date (inclusive) of the assignment.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} endDate
 *   Optional.
 *
 *   The expected or actual end date (inclusive) of the assignment.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {number} state
 *   Optional.
 *
 *   The current status of the assignment, identifiying whether the assignment
 *   is in progress or completed. Used for filtering on current assignment
 *   state.
 *
 *   The number should be among the values of [AssignmentState]{@link google.cloud.talent.v4beta1.AssignmentState}
 *
 * @property {string} languageCode
 *   Optional.
 *
 *   The language in which this assignment information is provided.
 *   This is distinct from the language of the job posting or profile.
 *
 * @property {boolean} extended
 *   Optional.
 *
 *   If the assignment was extended beyond the initially planned end date, this
 *   should be set to true; false otherwise. Update the corresponding end date
 *   if setting this field to true.
 *
 * @property {number} employerOutcome
 *   Optional.
 *
 *   The termination status of the assignment from the employer's perspective,
 *   if available and the assignment has termination. That is, an outcome of
 *   NEGATIVE means the employer would be unwilling to engage with this person
 *   on this or similar assignments again.
 *
 *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
 *
 * @property {number} assigneeOutcome
 *   Optional.
 *
 *   The termination status of the assignment from the assignee or candidate's
 *   perspective, if available and the assignment has termination.
 *   That is, an outcome of NEGATIVE means the candidate would be unwilling to
 *   engage with this employer or assignment again.
 *
 *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
 *
 * @property {string} outcomeNotes
 *   Optional.
 *
 *   Any notes, raw status codes, or additional context around the termination
 *   of the assignment.
 *
 * @property {Object} compensation
 *   Optional.
 *
 *   Any known candidate's compensation information for this assignment (salary,
 *   bonuses, and so on) where different from the associated job_id's
 *   compensation information (where available).
 *
 *   This object should have the same structure as [CompensationInfo]{@link google.cloud.talent.v4beta1.CompensationInfo}
 *
 * @property {Object} issuedFirstPaycheck
 *   Optional.
 *
 *   If true, the assignee has been issued the first paycheck for this
 *   assignment or not.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {number} assignmentType
 *   Optional.
 *
 *   The assignment's employment type (for example, PERMANENT, TEMPORARY)
 *   if different from the EmploymentType on the job.
 *
 *   The number should be among the values of [EmploymentType]{@link google.cloud.talent.v4beta1.EmploymentType}
 *
 * @property {Object} isSupervisor
 *   Optional.
 *
 *   If it is a supervisor position.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {number} supervisedEmployeeCount
 *   Optional.
 *
 *   The number of employees this person has supervised.
 *
 * @property {Object[]} performanceReviews
 *   Optional.
 *
 *   All performance feedback received by the employee (for example, performance
 *   reviews, and so on).
 *
 *   This object should have the same structure as [PerformanceReview]{@link google.cloud.talent.v4beta1.PerformanceReview}
 *
 * @property {Object[]} employeeSatisfactionSurveys
 *   Optional.
 *
 *   Any employee satisfaction surveys (for example, how happy they are with the
 *   role, and do on).
 *
 *   This object should have the same structure as [EmployeeSatisfactionSurvey]{@link google.cloud.talent.v4beta1.EmployeeSatisfactionSurvey}
 *
 * @typedef Assignment
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Assignment definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/assignment.proto}
 */
const Assignment = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * Enum that represents the assignment status.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  AssignmentState: {

    /**
     * Default value.
     */
    ASSIGNMENT_STATE_UNSPECIFIED: 0,

    /**
     * The person has not started the assignment yet.
     */
    NOT_STARTED: 1,

    /**
     * The person is currently on assignment.
     */
    IN_PROGRESS: 2,

    /**
     * The person has completed assignment.
     */
    COMPLETED: 3
  }
};

/**
 * All performance feedback received by the employee (i.e., performance reviews,
 * and so on).
 *
 * @property {Object} reviewTime
 *   Required.
 *
 *   Customer provided timestamp of when the performance review was given.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {string} reviewerId
 *   Unique identifier of the person providing the performance review.
 *
 * @property {number} reviewerRole
 *   Role of the person providing the performance review.
 *
 *   The number should be among the values of [ReviewerRole]{@link google.cloud.talent.v4beta1.ReviewerRole}
 *
 * @property {Object} reviewPeriodStartDate
 *   Required.
 *
 *   The first date (inclusive) of the period for which this performance review
 *   is for.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} reviewPeriodEndDate
 *   Required.
 *
 *   The last date (inclusive) of the period for which this performance review
 *   is for.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {string} reviewNotes
 *   Any free text notes on the performance review.
 *
 * @property {Object} rating
 *   The rating on this assessment.
 *
 *   This object should have the same structure as [Rating]{@link google.cloud.talent.v4beta1.Rating}
 *
 * @property {number} outcome
 *   Required.
 *
 *   Is the rating on this assessment area positive, negative, neutral?
 *
 *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
 *
 * @property {Object[]} performanceReviewTopics
 *   Optional.
 *
 *   Individual sub-components of the performance review (for example,
 *   communication skills, technical skills, and so on).
 *
 *   This object should have the same structure as [PerformanceReviewTopic]{@link google.cloud.talent.v4beta1.PerformanceReviewTopic}
 *
 * @typedef PerformanceReview
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.PerformanceReview definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/assignment.proto}
 */
const PerformanceReview = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * Individual sub-components of the performance review (for example,
   * communication skills, technical skills, and so on).
   *
   * @property {string} topicType
   *   Required.
   *
   *   What area of feedback this is about (for example, overall, communication
   *   skills, technical skills, and so on).
   *
   * @property {string} reviewerId
   *   Unique identifier of the person providing the performance review for this
   *   topic.
   *
   * @property {number} reviewerRole
   *   Role of the person providing the performance review for this topic.
   *
   *   The number should be among the values of [ReviewerRole]{@link google.cloud.talent.v4beta1.ReviewerRole}
   *
   * @property {string} reviewNotes
   *   Notes about the employee's performance on this sub-area.
   *
   * @property {Object} rating
   *   The rating on this assessment.
   *
   *   This object should have the same structure as [Rating]{@link google.cloud.talent.v4beta1.Rating}
   *
   * @property {number} outcome
   *   Required.
   *
   *   Is the rating on this assessment area positive, negative, neutral?
   *
   *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
   *
   * @typedef PerformanceReviewTopic
   * @memberof google.cloud.talent.v4beta1
   * @see [google.cloud.talent.v4beta1.PerformanceReview.PerformanceReviewTopic definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/assignment.proto}
   */
  PerformanceReviewTopic: {
    // This is for documentation. Actual contents will be loaded by gRPC.
  }
};

/**
 * Any employee satisfaction surveys (i.e., how happy they are with the role,
 * and so on).
 *
 * @property {Object} surveyTime
 *   Required.
 *
 *   Customer provided timestamp of when the satisfaction survey was
 *   completed.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {string} surveyName
 *   Required.
 *
 *   Customer provided survey name or description.
 *
 * @property {string} surveyNotes
 *   Any free-text notes / feedback provided on the survey.
 *
 * @property {Object} rating
 *   The rating on this assessment.
 *
 *   This object should have the same structure as [Rating]{@link google.cloud.talent.v4beta1.Rating}
 *
 * @property {number} outcome
 *   Required.
 *
 *   Is the rating on this assessment area positive, negative, neutral?
 *
 *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
 *
 * @property {Object[]} surveyTopics
 *   Optional.
 *
 *   Individual sub-components of the satisfaction survey (that is, overall,
 *   office location, manager, work / life balance, and so on).
 *
 *   This object should have the same structure as [EmployeeSatisfactionSurveyTopic]{@link google.cloud.talent.v4beta1.EmployeeSatisfactionSurveyTopic}
 *
 * @typedef EmployeeSatisfactionSurvey
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.EmployeeSatisfactionSurvey definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/assignment.proto}
 */
const EmployeeSatisfactionSurvey = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * Individual sub-components of the satisfaction survey (i.e., overall, office
   * location, manager, work / life balance, and so on).
   *
   * @property {string} surveyTopicName
   *   Required.
   *
   *   What area of satisfaction this is about (that is, overall, office
   *   location, manager, work / life balance, and so on).
   *
   * @property {string} surveyTopicNotes
   *   Any free-text notes / feedback provided on this sub-component.
   *
   * @property {Object} rating
   *   The rating on this assessment.
   *
   *   This object should have the same structure as [Rating]{@link google.cloud.talent.v4beta1.Rating}
   *
   * @property {number} outcome
   *   Is the rating on this assessment area positive, negative, neutral?
   *
   *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
   *
   * @typedef EmployeeSatisfactionSurveyTopic
   * @memberof google.cloud.talent.v4beta1
   * @see [google.cloud.talent.v4beta1.EmployeeSatisfactionSurvey.EmployeeSatisfactionSurveyTopic definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/assignment.proto}
   */
  EmployeeSatisfactionSurveyTopic: {
    // This is for documentation. Actual contents will be loaded by gRPC.
  }
};