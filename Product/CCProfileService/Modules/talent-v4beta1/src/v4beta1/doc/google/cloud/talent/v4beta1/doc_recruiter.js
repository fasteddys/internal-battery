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
 * A resource that represents a recruiter.
 *
 * @property {string} name
 *   Required during recruiter update.
 *
 *   Resource name assigned to a recruiter by the API.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/recruiters/{recruiter_id}",
 *   for example, "projects/api-test-project/tenants/foo/recruiters/bar".
 *
 * @property {string} externalId
 *   Required.
 *
 *   Client side recruiter identifier, used to uniquely identify the
 *   recruiter.
 *
 *   The maximum number of allowed characters is 255.
 *
 * @property {Object.<string, Object>} customAttributes
 *   Optional.
 *
 *   A map of fields to hold both filterable and non-filterable custom profile
 *   attributes that aren't covered by the provided structured fields. See
 *   CustomAttribute for more details.
 *
 *   At most 100 filterable and at most 100 unfilterable keys are supported. If
 *   limit is exceeded, an error is thrown.
 *
 *   Numeric custom attributes: each key can only map to one numeric value,
 *   otherwise an error is thrown.
 *
 *   String custom attributes: each key can map up to 50 string values. For
 *   filterable string value, each value has a byte size of no more than 256B.
 *   For unfilterable string values, the maximum byte size of a single key is
 *   64B. An error is thrown for any request exceeding the limit.
 *   The maximum total byte size is 10KB.
 *
 *   Currently filterable numeric custom attributes are not supported, and
 *   they automatically set to unfilterable.
 *
 * @property {string[]} profileTags
 *   Output only. The profile tags this recruiter manages.
 *
 * @typedef Recruiter
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Recruiter definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/recruiter.proto}
 */
const Recruiter = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};