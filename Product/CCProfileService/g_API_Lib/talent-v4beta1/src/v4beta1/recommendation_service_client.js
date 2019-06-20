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

    // Some of the methods on this service return "paged" results,
    // (e.g. 50 results at a time, with tokens to get subsequent
    // pages). Denote the keys used for pagination and results.
    this._descriptors.page = {
      recommendProfilesForJob: new gax.PageDescriptor(
        'pageToken',
        'nextPageToken',
        'summarizedProfiles'
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
      'recommendProfilesForJob',
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
        this._descriptors.page[methodName]
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
   * @param {string} [request.job]
   *   The resource name for the job to get recommendations from. This is
   *   generated for the service when a job is created.
   *
   *   The format is "projects/{project_id}/jobs/{job_id}", for example,
   *   "projects/api-test-project/jobs/1234".
   * @param {number} [request.pageSize]
   *   The maximum number of resources contained in the underlying API
   *   response. If page streaming is performed per-resource, this
   *   parameter does not affect the return value. If page streaming is
   *   performed per-page, this determines the maximum number of
   *   resources in a page.
   * @param {Object} [request.profileQuery]
   *   Optional.
   *
   *   Profile query to filter profiles for recommendation.
   *
   *   This object should have the same structure as [ProfileQuery]{@link google.cloud.talent.v4beta1.ProfileQuery}
   * @param {number} [request.offset]
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
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error, ?Array, ?Object, ?Object)} [callback]
   *   The function which will be called with the result of the API call.
   *
   *   The second parameter to the callback is Array of [SummarizedProfile]{@link google.cloud.talent.v4beta1.SummarizedProfile}.
   *
   *   When autoPaginate: false is specified through options, it contains the result
   *   in a single response. If the response indicates the next page exists, the third
   *   parameter is set to be used for the next request object. The fourth parameter keeps
   *   the raw response object of an object representing [RecommendProfilesForJobResponse]{@link google.cloud.talent.v4beta1.RecommendProfilesForJobResponse}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is Array of [SummarizedProfile]{@link google.cloud.talent.v4beta1.SummarizedProfile}.
   *
   *   When autoPaginate: false is specified through options, the array has three elements.
   *   The first element is Array of [SummarizedProfile]{@link google.cloud.talent.v4beta1.SummarizedProfile} in a single response.
   *   The second element is the next request object if the response
   *   indicates the next page exists, or null. The third element is
   *   an object representing [RecommendProfilesForJobResponse]{@link google.cloud.talent.v4beta1.RecommendProfilesForJobResponse}.
   *
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
   * // Iterate over all elements.
   * client.recommendProfilesForJob({})
   *   .then(responses => {
   *     const resources = responses[0];
   *     for (const resource of resources) {
   *       // doThingsWith(resource)
   *     }
   *   })
   *   .catch(err => {
   *     console.error(err);
   *   });
   *
   * // Or obtain the paged response.
   *
   * const options = {autoPaginate: false};
   * const callback = responses => {
   *   // The actual resources in a response.
   *   const resources = responses[0];
   *   // The next request if the response shows that there are more responses.
   *   const nextRequest = responses[1];
   *   // The actual response object, if necessary.
   *   // const rawResponse = responses[2];
   *   for (const resource of resources) {
   *     // doThingsWith(resource);
   *   }
   *   if (nextRequest) {
   *     // Fetch the next page.
   *     return client.recommendProfilesForJob(nextRequest, options).then(callback);
   *   }
   * }
   * client.recommendProfilesForJob({}, options)
   *   .then(callback)
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  recommendProfilesForJob(request, options, callback) {
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

    return this._innerApiCalls.recommendProfilesForJob(request, options, callback);
  }

  /**
   * Equivalent to {@link recommendProfilesForJob}, but returns a NodeJS Stream object.
   *
   * This fetches the paged responses for {@link recommendProfilesForJob} continuously
   * and invokes the callback registered for 'data' event for each element in the
   * responses.
   *
   * The returned object has 'end' method when no more elements are required.
   *
   * autoPaginate option will be ignored.
   *
   * @see {@link https://nodejs.org/api/stream.html}
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} [request.job]
   *   The resource name for the job to get recommendations from. This is
   *   generated for the service when a job is created.
   *
   *   The format is "projects/{project_id}/jobs/{job_id}", for example,
   *   "projects/api-test-project/jobs/1234".
   * @param {number} [request.pageSize]
   *   The maximum number of resources contained in the underlying API
   *   response. If page streaming is performed per-resource, this
   *   parameter does not affect the return value. If page streaming is
   *   performed per-page, this determines the maximum number of
   *   resources in a page.
   * @param {Object} [request.profileQuery]
   *   Optional.
   *
   *   Profile query to filter profiles for recommendation.
   *
   *   This object should have the same structure as [ProfileQuery]{@link google.cloud.talent.v4beta1.ProfileQuery}
   * @param {number} [request.offset]
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
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @returns {Stream}
   *   An object stream which emits an object representing [SummarizedProfile]{@link google.cloud.talent.v4beta1.SummarizedProfile} on 'data' event.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.RecommendationServiceClient({
   *   // optional auth parameters.
   * });
   *
   *
   * client.recommendProfilesForJobStream({})
   *   .on('data', element => {
   *     // doThingsWith(element)
   *   }).on('error', err => {
   *     console.log(err);
   *   });
   */
  recommendProfilesForJobStream(request, options) {
    options = options || {};

    return this._descriptors.page.recommendProfilesForJob.createStream(
      this._innerApiCalls.recommendProfilesForJob,
      request,
      options
    );
  };
}


module.exports = RecommendationServiceClient;
