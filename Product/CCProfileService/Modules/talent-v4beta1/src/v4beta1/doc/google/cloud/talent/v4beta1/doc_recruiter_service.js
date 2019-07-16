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
 * List recruiters request.
 *
 * @property {string} parent
 *   Required.
 *
 *   The resource name of the tenant under which the recruiter is created.
 *
 *   The format is "projects/{project_id}/tenants/{tenant_id}", for example,
 *   "projects/api-test-project/tenants/test-tenant".
 *
 * @property {string} pageToken
 *   Optional.
 *
 *   The token that specifies the current offset (that is, starting result).
 *
 *   Please set the value to ListRecruitersResponse.next_page_token to
 *   continue the list.
 *
 * @property {number} pageSize
 *   Optional.
 *
 *   The maximum number of recruiters to be returned, at most 100.
 *
 *   Default is 100 unless a positive number smaller than 100 is specified.
 *
 * @typedef ListRecruitersRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.ListRecruitersRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/recruiter_service.proto}
 */
const ListRecruitersRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * The List recruiters response object.
 *
 * @property {Object[]} recruiters
 *   Recruiters for the specific company.
 *
 *   This object should have the same structure as [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}
 *
 * @property {string} nextPageToken
 *   A token to retrieve the next page of results. This is empty if there are no
 *   more results.
 *
 * @property {Object} metadata
 *   Additional information for the API invocation, such as the request
 *   tracking id.
 *
 *   This object should have the same structure as [ResponseMetadata]{@link google.cloud.talent.v4beta1.ResponseMetadata}
 *
 * @typedef ListRecruitersResponse
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.ListRecruitersResponse definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/recruiter_service.proto}
 */
const ListRecruitersResponse = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Create recruiter request.
 *
 * @property {string} parent
 *   Required.
 *
 *   The name of the company this recruiter belongs to.
 *
 *   The format is "projects/{project_id}", for example,
 *   "projects/api-test-project".
 *
 * @property {Object} recruiter
 *   Required.
 *
 *   The recruiter to be created.
 *
 *   This object should have the same structure as [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}
 *
 * @typedef CreateRecruiterRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.CreateRecruiterRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/recruiter_service.proto}
 */
const CreateRecruiterRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Get recruiter request.
 *
 * @property {string} name
 *   Required.
 *
 *   Resource name of the recruiter to get.
 *
 *   The format is
 *   "projects/{project_id}/recruiters/{recruiter_id}",
 *   for example, "projects/api-test-project/recruiters/bar".
 *
 * @typedef GetRecruiterRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.GetRecruiterRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/recruiter_service.proto}
 */
const GetRecruiterRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Update recruiter request
 *
 * @property {Object} recruiter
 *   Required.
 *
 *   Recruiter to be updated.
 *
 *   This object should have the same structure as [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}
 *
 * @property {Object} updateMask
 *   Optional.
 *
 *   A field mask to specify the recruiter fields to update.
 *
 *   A full update is performed if it is unset.
 *
 *   Valid values are:
 *
 *   * customAttributes
 *
 *   This object should have the same structure as [FieldMask]{@link google.protobuf.FieldMask}
 *
 * @typedef UpdateRecruiterRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.UpdateRecruiterRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/recruiter_service.proto}
 */
const UpdateRecruiterRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Delete recruiter request.
 *
 * @property {string} name
 *   Required.
 *
 *   Resource name of the recruiter to be deleted.
 *
 *   The format is
 *   "projects/{project_id}/recruiters/{recruiter_id}",
 *   for example, "projects/api-test-project/recruiters/bar".
 *
 * @typedef DeleteRecruiterRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.DeleteRecruiterRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/recruiter_service.proto}
 */
const DeleteRecruiterRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Add profile tags request.
 *
 * @property {string} recruiter
 *   Required.
 *
 *   Resource name assigned to a recruiter by the API.
 *
 *   The format is
 *   "projects/{project_id}}/recruiters/{recruiter_id}",
 *   for example, "projects/api-test-project/recruiters/bar".
 *
 * @property {string[]} profileTags
 *   Required.
 *
 *   The profile tags added to the recruiter's list of profile tags.
 *
 * @typedef AddRecruiterProfileTagsRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.AddRecruiterProfileTagsRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/recruiter_service.proto}
 */
const AddRecruiterProfileTagsRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Remove profile tags request.
 *
 * @property {string} recruiter
 *   Required.
 *
 *   Resource name assigned to a recruiter by the API.
 *
 *   The format is
 *   "projects/{project_id}/recruiters/{recruiter_id}",
 *   for example, "projects/api-test-project/recruiters/bar".
 *
 * @property {string[]} profileTags
 *   Required.
 *
 *   The profile tags removed from the recruiter's list of profile tags.
 *
 * @typedef RemoveRecruiterProfileTagsRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.RemoveRecruiterProfileTagsRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/recruiter_service.proto}
 */
const RemoveRecruiterProfileTagsRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};