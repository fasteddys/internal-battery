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

'use strict';

const gapicConfig = require('./recommendation_service_client_config.json');
const gax = require('google-gax');
const merge = require('lodash.merge');
const path = require('path');

const VERSION = require('../../package.json').version;

/**
 * A service handles recommendations between jobs and profiles.
 *
 * @class
 * @memberof v4beta1
 */
class RecommendationServiceClient {
  /**
   * Construct an instance of RecommendationServiceClient.
   *
   * @param {object} [options] - The configuration object. See the subsequent
   *   parameters for more details.
   * @param {object} [options.credentials] - Credentials object.
   * @param {string} [options.credentials.client_email]
   * @param {string} [options.credentials.private_key]
   * @param {string} [options.email] - Account email address. Required when
   *     using a .pem or .p12 keyFilename.
   * @param {string} [options.keyFilename] - Full path to the a .json, .pem, or
   *     .p12 key downloaded from the Google Developers Console. If you provide
   *     a path to a JSON file, the projectId option below is not necessary.
   *     NOTE: .pem and .p12 require you to specify options.email as well.
   * @param {number} [options.port] - The port on which to connect to
   *     the remote host.
   * @param {string} [options.projectId] - The project ID from the Google
   *     Developer's Console, e.g. 'grape-spaceship-123'. We will also check
   *     the environment variable GCLOUD_PROJECT for your project ID. If your
   *     app is running in an environment which supports
   *     {@link https://developers.google.com/identity/protocols/application-default-credentials Application Default Credentials},
   *     your project ID will be detected automatically.
   * @param {function} [options.promise] - Custom promise module to use instead
   *     of native Promises.
   * @param {string} [options.servicePath] - The domain name of the
   *     API remote host.
   */
  constructor(opts) {
    this._descriptors = {};

    // Ensure that options include the service address and port.
    opts = Object.assign(
      {
        clientConfig: {},
        port: this.constructor.port,
        servicePath: this.constructor.servicePath,
      },
      opts
    );

    // Create a `gaxGrpc` object, with any grpc-specific options
    // sent to the client.
    opts.scopes = this.constructor.scopes;
    const gaxGrpc = new gax.GrpcClient(opts);

    // Save the auth object to the client, for use by other methods.
    this.auth = gaxGrpc.auth;

    // Determine the client header string.
    const clientHeader = [
      `gl-node/${process.version}`,
      `grpc/${gaxGrpc.grpcVersion}`,
      `gax/${gax.version}`,
      `gapic/${VERSION}`,
    ];
    if (opts.libName && opts.libVersion) {
      clientHeader.push(`${opts.libName}/${opts.libVersion}`);
    }

    // Load the applicable protos.
    const protos = merge(
      {},
      gaxGrpc.loadProto(
        path.join(__dirname, '..', '..', 'protos'),
        'google/cloud/talent/v4beta1/recommendation_service.proto'
      )
    );

    // This API contains "path templates"; forward-slash-separated
    // identifiers to uniquely identify resources within the API.
    // Create useful helper objects for these.
    this._pathTemplates = {
      jobPathTemplate: new gax.PathTemplate(
        'projects/{project}/tenants/{tenant}/jobs/{jobs}'
      ),
      companyPathTemplate: new gax.PathTemplate(
        'projects/{project}/tenants/{tenant}/companies/{company}'
      ),
    };

    // Put together the default options sent with requests.
    const defaults = gaxGrpc.constructSettings(
      'google.cloud.talent.v4beta1.RecommendationService',
      gapicConfig,
      opts.clientConfig,
      {'x-goog-api-client': clientHeader.join(' ')}
    );

    // Set up a dictionary of "inner API calls"; the core implementation
    // of calling the API is handled in `google-gax`, with this code
    // merely providing the destination and request information.
    this._innerApiCalls = {};

    // Put together the "service stub" for
    // google.cloud.talent.v4beta1.RecommendationService.
    const recommendationServiceStub = gaxGrpc.createStub(
      protos.google.cloud.talent.v4beta1.RecommendationService,
      opts
    );

    // Iterate over each of the methods that the service provides
    // and create an API call method for each.
    const recommendationServiceStubMethods = [
      'recommendProfiles',
    ];
    for (const methodName of recommendationServiceStubMethods) {
      this._innerApiCalls[methodName] = gax.createApiCall(
        recommendationServiceStub.then(
          stub =>
            function() {
              const args = Array.prototype.slice.call(arguments, 0);
              return stub[methodName].apply(stub, args);
            },
          err =>
            function() {
              throw err;
            }
        ),
        defaults[methodName],
        null
      );
    }
  }

  /**
   * The DNS address for this API service.
   */
  static get servicePath() {
    return 'jobs.googleapis.com';
  }

  /**
   * The port for this API service.
   */
  static get port() {
    return 443;
  }

  /**
   * The scopes needed to make gRPC calls for every method defined
   * in this service.
   */
  static get scopes() {
    return [
      'https://www.googleapis.com/auth/cloud-platform',
      'https://www.googleapis.com/auth/jobs',
    ];
  }

  /**
   * Return the project ID used by this class.
   * @param {function(Error, string)} callback - the callback to
   *   be called with the current project Id.
   */
  getProjectId(callback) {
    return this.auth.getProjectId(callback);
  }

  // -------------------
  // -- Service calls --
  // -------------------

  /**
   * Recommend profiles for a given job.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} request.recruiter
   *   Required.
   *
   *   The identifier of the recruiter.
   *   The format is
   *   "projects/{project_id}}/tenants/{tenant_id}/recruiters/{recruiter_id}",
   *   for example, "projects/api-test-project/tenants/foo/recruiters/bar".
   * @param {string} [request.job]
   *   The job name for get recommendations for.
   *   The format is "projects/{project_id}/tenants/{tenant_id}/jobs/{job_id}",
   *   for example, "projects/api-test-project/tenants/foo/jobs/bar".
   * @param {Object} [request.profileQuery]
   *   Optional.
   *
   *   Profile query to filter profiles for recommendation.  Currently, only
   *   location_filters,
   *   custom_attribute_filter, and
   *   candidate_availability_filter
   *   are supported.
   *
   *   This object should have the same structure as [ProfileQuery]{@link google.cloud.talent.v4beta1.ProfileQuery}
   * @param {number} [request.pageSize]
   *   Optional.
   *
   *   The maximum number of profiles to be returned per page of results.
   *
   *   The maximum allowed page size is 50.  Default is 10.
   * @param {string} [request.pageToken]
   *   Optional.
   *
   *   The starting point of a query result.
   *
   *   If page_token is set, then result_set_id must also be set, and
   *   offset must be unset.  Otherwise INVALID_ARGUMENT error will be
   *   returned.
   * @param {string} [request.resultSetId]
   *   Optional.
   *
   *   An id that uniquely identifies the result set of a
   *   RecommendProfiles call.
   *   The id should be retrieved from the
   *   RecommendProfilesResponse message returned from a previous
   *   invocation of RecommendProfiles.
   *
   *    The use case for this feature is to ensure that the underlying results and
   *    the order of those results do not change during a usage session (an
   *    example usage session: an end user requesting recommendations, reviewing
   *    those recommendations by paging back & forth between the pages of results,
   *    and actioning each of those candidates).
   *
   *      We suggest that you provide a UX affordance to enable the end user to
   *      refresh the results (in other words, compute a new result set).
   *
   *      A result_set_id is valid for up to 30 minutes, meaning that the
   *      results set returned will be viewable for that time period.  Each time a
   *      result_set_id is accessed, it is extended for 30 minutes. This
   *      action adds a new 30 minute time block to the end of the existing time
   *      block regardless of how much time as passed. For example, if a
   *      result_set_id is created at minute 1, then accessed at minute 29,
   *      the cursor will be available until minute 59, rather than expiring at
   *      minute 30 had it not been accessed.  The maximum time that a
   *      result_set_id can be valid is 12 hours from the initial creation.  A
   *      NOT_FOUND error is thrown if the result_set_id is expired or
   *      invalid.
   *
   *      The result_set_id only caches the [insert field name of profile ID].
   *      That is, if a Profile in the result_set_id is updated, the page of
   *      results containing that Profile will reflect any changes via the CUD
   *      APIs when it is returned (although changes will NOT impact which results
   *      are returned or the order of those results).  For example, if a
   *      Profile's first_name was changed from "ammy" to "amy", the "amy" will be
   *      returned in any subsequent calls.
   *
   *    Without the use of this feature, the end user could experience a scenario
   *    in which the set of candidates displayed or the order of candidates
   *    displayed could change as they page back and forth. The jobs, profiles,
   *    applications, assignments, and other objects stored in CTS could be
   *    updated via the CUD APIs (for example updating candidate profiles,
   *    assignments, applications, and so on) and those changes would cause the
   *    set of candidates recommended for a given job to change.
   *
   *      Consider an example:
   *   At T = 0, user requests recommendations for Job A with a page size of 5
   *
   *   WITHOUT USE OF THIS FEATURE
   *
   *   At T = 0, user views the first page of results which shows
   *
   *   * Candidate 1
   *   * 2
   *   * 3
   *   * 4
   *   * Candidate 5
   *
   *   At T = 1, user requests the second page of results
   *
   *   * Candidate 6
   *   * 7
   *   * 8
   *   * 9
   *   * Candidate 10
   *
   *   At T = 2, profile for candidate #10 is updated via the CUD API (which
   *   causes candidate #10 to become less relevant for Job A) At T = 3, profile
   *   for candidate #2 is deleted via the CUD API
   *
   *   At T = 4, user requests the first page of results (again)
   *
   *   * Candidate 1
   *   * 3
   *   * 4
   *   * 5
   *   * Candidate 6 <--- NOTE: Candidate 6 moves from page #2 to page #1
   *
   *   At T = 5, user requests the second page of results (again)
   *
   *   * Candidate 7 <--- NOTE: Candidate 7 is now the first result on page #2
   *   * 8
   *   * 9
   *   * 11
   *   * Candidate 12 <--- NOTE: Candidate 10 is no longer on page #2 because they
   *   are now less relevant than candidate #12
   *
   *   WITH USE OF THIS FEATURE
   *
   *   At T = 0, user views the first page of results which shows
   *
   *   * Candidate 1
   *   * 2
   *   * 3
   *   * 4
   *   * Candidate 5
   *
   *   At T = 1, user requests the second page of results
   *
   *   * Candidate 6
   *   * 7
   *   * 8
   *   * 9
   *   * Candidate 10
   *
   *   At T = 2, profile for candidate #10 is updated via the CUD API (which
   *   causes candidate #10 to become less relevant for Job A) At T = 3, profile
   *   for candidate #2 is deleted via the CUD API
   *
   *   At T = 4, user requests the first page of results (again)
   *
   *   * Candidate 1
   *   * 3
   *   * 4
   *   * Candidate 5
   *
   *   Note that the page shows only 4 results instead of 5, because candidate
   *   # 2 was deleted at T=2.
   *
   *   At T = 5, user requests the second page of results (again)
   *
   *   * Candidate 6
   *   * 7
   *   * 8
   *   * 9
   *   * Candidate 10 <--- NOTE: Candidate 10 is still displayed in the original
   *   position even though they are less relevant (although, the information
   *   returned about Candidate 10 reflects all updates made at T = 2).
   *
   *   At T = 6, user presses the UI affordance to 'refresh' the results for this
   *   query (which results in the same query being issued to the API with a blank
   *   result_set_id)
   *
   *   At T = 7, user requests the first page of results (which reflect all
   *   changes)
   *
   *   * Candidate 1
   *   * 2
   *   * 3
   *   * 4
   *   * Candidate 5
   *
   *   At T = 8, user requests the second page of results (which reflect all
   *   changes)
   *
   *   * Candidate 7
   *   * 8
   *   * 9
   *   * 11
   *   * Candidate 12
   *
   *   If this field is not set, a new result set is computed based on the
   *   resource (for example job) and the profile_query.  A new
   *   result_set_id is returned as a handle to access this result set.
   *
   *   If this field is set, the service will ignore the resource and
   *   profile_query values, and simply retrieve a page of results from the
   *   corresponding result set.  In this case, one and only one of page_token
   *   or offset must be set.
   *
   *   A typical use case is to invoke RecommendProfilesRequest without this
   *   field, then use the resulting result_set_id in
   *   RecommendProfilesResponse to page through the results.
   *
   *   Because candidates may be deleted after a result set is created, certain
   *   paging requests may receive a response with less number of results than
   *   requested.  This is to guarantee that the same candidate always appear
   *   on the same page even if some candidates are deleted.
   * @param {number} [request.offset]
   *   Optional.
   *
   *   An integer that specifies the current offset (that is, starting result) in
   *   search results.
   *
   *   If offset is set, page_token must be unset.  Otherwise
   *   INVALID_ARGUMENT error will be returned.
   *
   *   The maximum allowed value is 250. Otherwise an error is thrown.
   *
   *   For example, 0 means to search from the first profile, and 10 means to
   *   search from the 11th profile. This can be used for pagination, for example
   *   pageSize = 10 and offset = 10 means to search from the second page.
   * @param {Object} [request.fieldMask]
   *   Optional.
   *
   *   A field mask to specify the profile fields to be included in the
   *   SummarizedProfile in the results.  The field
   *   Profile.name is always included.
   *   You can specify one or more of the following fields:
   *
   *   * external_id
   *   * external_system
   *   * source
   *   * uri
   *   * source_note
   *   * group_id
   *   * field_update_metadata
   *   * is_hirable
   *   * create_time
   *   * update_time
   *   * resume
   *   * primary_responsibilities
   *   * citizenships
   *   * work_authorizations
   *   * employee_types
   *   * language_code
   *   * qualification_summary
   *   * person_names
   *   * addresses
   *   * email_addresses
   *   * phone_numbers
   *   * personal_uris
   *   * additional_contact_info
   *   * allowed_contact_types
   *   * preferred_contact_types
   *   * contact_availability
   *   * employment_records
   *   * education_records
   *   * skills
   *   * language_fluencies
   *   * activities
   *   * publications
   *   * patents
   *   * certifications
   *   * applications
   *   * assignments
   *   * recruiting_notes
   *   * work_preference
   *   * industry_experiences
   *   * work_environment_experiences
   *   * work_availability
   *   * security_clearances
   *   * custom_attributes
   *   * tags
   *   * currently_placed
   *   * applied
   *   * previously_placed
   *   * summarized_employment_history
   *
   *   This object should have the same structure as [FieldMask]{@link google.protobuf.FieldMask}
   * @param {Object} [request.requestMetadata]
   *   Optional.
   *
   *   The meta information collected about the user of the service. This is used
   *   to enable certain features and to improve the service quality. These
   *   values are provided by users, and must be precise and consistent.
   *
   *   This object should have the same structure as [RequestMetadata]{@link google.cloud.talent.v4beta1.RequestMetadata}
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error, ?Object)} [callback]
   *   The function which will be called with the result of the API call.
   *
   *   The second parameter to the callback is an object representing [RecommendProfilesResponse]{@link google.cloud.talent.v4beta1.RecommendProfilesResponse}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is an object representing [RecommendProfilesResponse]{@link google.cloud.talent.v4beta1.RecommendProfilesResponse}.
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.RecommendationServiceClient({
   *   // optional auth parameters.
   * });
   *
   * const recruiter = '';
   * client.recommendProfiles({recruiter: recruiter})
   *   .then(responses => {
   *     const response = responses[0];
   *     // doThingsWith(response)
   *   })
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  recommendProfiles(request, options, callback) {
    if (options instanceof Function && callback === undefined) {
      callback = options;
      options = {};
    }
    options = options || {};
    options.otherArgs = options.otherArgs || {};
    options.otherArgs.headers = options.otherArgs.headers || {};
    options.otherArgs.headers['x-goog-request-params'] =
      gax.routingHeader.fromParams({
        'job': request.job
      });

    return this._innerApiCalls.recommendProfiles(request, options, callback);
  }

  // --------------------
  // -- Path templates --
  // --------------------

  /**
   * Return a fully-qualified job resource name string.
   *
   * @param {String} project
   * @param {String} tenant
   * @param {String} jobs
   * @returns {String}
   */
  jobPath(project, tenant, jobs) {
    return this._pathTemplates.jobPathTemplate.render({
      project: project,
      tenant: tenant,
      jobs: jobs,
    });
  }

  /**
   * Return a fully-qualified company resource name string.
   *
   * @param {String} project
   * @param {String} tenant
   * @param {String} company
   * @returns {String}
   */
  companyPath(project, tenant, company) {
    return this._pathTemplates.companyPathTemplate.render({
      project: project,
      tenant: tenant,
      company: company,
    });
  }

  /**
   * Parse the jobName from a job resource.
   *
   * @param {String} jobName
   *   A fully-qualified path representing a job resources.
   * @returns {String} - A string representing the project.
   */
  matchProjectFromJobName(jobName) {
    return this._pathTemplates.jobPathTemplate
      .match(jobName)
      .project;
  }

  /**
   * Parse the jobName from a job resource.
   *
   * @param {String} jobName
   *   A fully-qualified path representing a job resources.
   * @returns {String} - A string representing the tenant.
   */
  matchTenantFromJobName(jobName) {
    return this._pathTemplates.jobPathTemplate
      .match(jobName)
      .tenant;
  }

  /**
   * Parse the jobName from a job resource.
   *
   * @param {String} jobName
   *   A fully-qualified path representing a job resources.
   * @returns {String} - A string representing the jobs.
   */
  matchJobsFromJobName(jobName) {
    return this._pathTemplates.jobPathTemplate
      .match(jobName)
      .jobs;
  }

  /**
   * Parse the companyName from a company resource.
   *
   * @param {String} companyName
   *   A fully-qualified path representing a company resources.
   * @returns {String} - A string representing the project.
   */
  matchProjectFromCompanyName(companyName) {
    return this._pathTemplates.companyPathTemplate
      .match(companyName)
      .project;
  }

  /**
   * Parse the companyName from a company resource.
   *
   * @param {String} companyName
   *   A fully-qualified path representing a company resources.
   * @returns {String} - A string representing the tenant.
   */
  matchTenantFromCompanyName(companyName) {
    return this._pathTemplates.companyPathTemplate
      .match(companyName)
      .tenant;
  }

  /**
   * Parse the companyName from a company resource.
   *
   * @param {String} companyName
   *   A fully-qualified path representing a company resources.
   * @returns {String} - A string representing the company.
   */
  matchCompanyFromCompanyName(companyName) {
    return this._pathTemplates.companyPathTemplate
      .match(companyName)
      .company;
  }
}


module.exports = RecommendationServiceClient;
