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
 * Recommend profiles for job request.
 *
 * @property {string} job
 *   The resource name for the job to get recommendations from. This is
 *   generated for the service when a job is created.
 *
 *   The format is "projects/{project_id}/jobs/{job_id}", for example,
 *   "projects/api-test-project/jobs/1234".
 *
 * @property {string} pageToken
 *   Optional.
 *
 *   The starting point of a query result.
 *
 * @property {number} pageSize
 *   Optional.
 *
 *   The maximum number of profiles to be returned per page of results.
 *
 *   The maximum allowed page size is 100.
 *
 *   Default is 100 if unspecified.
 *
 * @property {Object} profileQuery
 *   Optional.
 *
 *   Profile query to filter profiles for recommendation.
 *
 *   This object should have the same structure as [ProfileQuery]{@link google.cloud.talent.v4beta1.ProfileQuery}
 *
 * @property {number} offset
 *   Optional.
 *
 *   An integer that specifies the current offset (that is, starting result) in
 *   search results. This field is only considered if page_token is unset.
 *
 *   The maximum allowed value is 5000. Otherwise an error is thrown.
 *
 *   For example, 0 means to search from the first profile, and 10 means to
 *   search from the 11th profile. This can be used for pagination, for example
 *   pageSize = 10 and offset = 10 means to search from the second page.
 *
 * @typedef RecommendProfilesForJobRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.RecommendProfilesForJobRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/recommendation_service.proto}
 */
const RecommendProfilesForJobRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Recommend profiles for job response.
 *
 * @property {Object[]} summarizedProfiles
 *   The profiles recommended according to the criteria in the request.
 *
 *   This object should have the same structure as [SummarizedProfile]{@link google.cloud.talent.v4beta1.SummarizedProfile}
 *
 * @property {string} nextPageToken
 *   A token to retrieve the next page of results.
 *
 * @typedef RecommendProfilesForJobResponse
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.RecommendProfilesForJobResponse definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/recommendation_service.proto}
 */
const RecommendProfilesForJobResponse = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};