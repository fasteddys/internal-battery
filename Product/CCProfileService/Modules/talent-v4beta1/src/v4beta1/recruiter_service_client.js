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

const gapicConfig = require('./recruiter_service_client_config.json');
const gax = require('google-gax');
const merge = require('lodash.merge');
const path = require('path');

const VERSION = require('../../package.json').version;

/**
 * A service that handles recruiter management.
 *
 * @class
 * @memberof v4beta1
 */
class RecruiterServiceClient {
  /**
   * Construct an instance of RecruiterServiceClient.
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
        'google/cloud/talent/v4beta1/recruiter_service.proto'
      )
    );

    // This API contains "path templates"; forward-slash-separated
    // identifiers to uniquely identify resources within the API.
    // Create useful helper objects for these.
    this._pathTemplates = {
      tenantPathTemplate: new gax.PathTemplate(
        'projects/{project}/tenants/{tenant}'
      ),
      recruiterPathTemplate: new gax.PathTemplate(
        'projects/{project}/tenants/{tenant}/recruiters/{recruiter}'
      ),
    };

    // Some of the methods on this service return "paged" results,
    // (e.g. 50 results at a time, with tokens to get subsequent
    // pages). Denote the keys used for pagination and results.
    this._descriptors.page = {
      listRecruiters: new gax.PageDescriptor(
        'pageToken',
        'nextPageToken',
        'recruiters'
      ),
    };

    // Put together the default options sent with requests.
    const defaults = gaxGrpc.constructSettings(
      'google.cloud.talent.v4beta1.RecruiterService',
      gapicConfig,
      opts.clientConfig,
      {'x-goog-api-client': clientHeader.join(' ')}
    );

    // Set up a dictionary of "inner API calls"; the core implementation
    // of calling the API is handled in `google-gax`, with this code
    // merely providing the destination and request information.
    this._innerApiCalls = {};

    // Put together the "service stub" for
    // google.cloud.talent.v4beta1.RecruiterService.
    const recruiterServiceStub = gaxGrpc.createStub(
      protos.google.cloud.talent.v4beta1.RecruiterService,
      opts
    );

    // Iterate over each of the methods that the service provides
    // and create an API call method for each.
    const recruiterServiceStubMethods = [
      'listRecruiters',
      'createRecruiter',
      'getRecruiter',
      'updateRecruiter',
      'deleteRecruiter',
      'addRecruiterProfileTags',
      'removeRecruiterProfileTags',
    ];
    for (const methodName of recruiterServiceStubMethods) {
      this._innerApiCalls[methodName] = gax.createApiCall(
        recruiterServiceStub.then(
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
   * Lists all recruiters associated with the tenant.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} request.parent
   *   Required.
   *
   *   The resource name of the tenant under which the recruiter is created.
   *
   *   The format is "projects/{project_id}/tenants/{tenant_id}", for example,
   *   "projects/api-test-project/tenants/test-tenant".
   * @param {number} [request.pageSize]
   *   The maximum number of resources contained in the underlying API
   *   response. If page streaming is performed per-resource, this
   *   parameter does not affect the return value. If page streaming is
   *   performed per-page, this determines the maximum number of
   *   resources in a page.
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error, ?Array, ?Object, ?Object)} [callback]
   *   The function which will be called with the result of the API call.
   *
   *   The second parameter to the callback is Array of [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   *
   *   When autoPaginate: false is specified through options, it contains the result
   *   in a single response. If the response indicates the next page exists, the third
   *   parameter is set to be used for the next request object. The fourth parameter keeps
   *   the raw response object of an object representing [ListRecruitersResponse]{@link google.cloud.talent.v4beta1.ListRecruitersResponse}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is Array of [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   *
   *   When autoPaginate: false is specified through options, the array has three elements.
   *   The first element is Array of [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter} in a single response.
   *   The second element is the next request object if the response
   *   indicates the next page exists, or null. The third element is
   *   an object representing [ListRecruitersResponse]{@link google.cloud.talent.v4beta1.ListRecruitersResponse}.
   *
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.RecruiterServiceClient({
   *   // optional auth parameters.
   * });
   *
   * // Iterate over all elements.
   * const formattedParent = client.tenantPath('[PROJECT]', '[TENANT]');
   *
   * client.listRecruiters({parent: formattedParent})
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
   * const formattedParent = client.tenantPath('[PROJECT]', '[TENANT]');
   *
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
   *     return client.listRecruiters(nextRequest, options).then(callback);
   *   }
   * }
   * client.listRecruiters({parent: formattedParent}, options)
   *   .then(callback)
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  listRecruiters(request, options, callback) {
    if (options instanceof Function && callback === undefined) {
      callback = options;
      options = {};
    }
    options = options || {};
    options.otherArgs = options.otherArgs || {};
    options.otherArgs.headers = options.otherArgs.headers || {};
    options.otherArgs.headers['x-goog-request-params'] =
      gax.routingHeader.fromParams({
        'parent': request.parent
      });

    return this._innerApiCalls.listRecruiters(request, options, callback);
  }

  /**
   * Equivalent to {@link listRecruiters}, but returns a NodeJS Stream object.
   *
   * This fetches the paged responses for {@link listRecruiters} continuously
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
   * @param {string} request.parent
   *   Required.
   *
   *   The resource name of the tenant under which the recruiter is created.
   *
   *   The format is "projects/{project_id}/tenants/{tenant_id}", for example,
   *   "projects/api-test-project/tenants/test-tenant".
   * @param {number} [request.pageSize]
   *   The maximum number of resources contained in the underlying API
   *   response. If page streaming is performed per-resource, this
   *   parameter does not affect the return value. If page streaming is
   *   performed per-page, this determines the maximum number of
   *   resources in a page.
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @returns {Stream}
   *   An object stream which emits an object representing [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter} on 'data' event.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.RecruiterServiceClient({
   *   // optional auth parameters.
   * });
   *
   * const formattedParent = client.tenantPath('[PROJECT]', '[TENANT]');
   * client.listRecruitersStream({parent: formattedParent})
   *   .on('data', element => {
   *     // doThingsWith(element)
   *   }).on('error', err => {
   *     console.log(err);
   *   });
   */
  listRecruitersStream(request, options) {
    options = options || {};

    return this._descriptors.page.listRecruiters.createStream(
      this._innerApiCalls.listRecruiters,
      request,
      options
    );
  };

  /**
   * Creates and returns a new recruiter.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} request.parent
   *   Required.
   *
   *   The name of the company this recruiter belongs to.
   *
   *   The format is "projects/{project_id}", for example,
   *   "projects/api-test-project".
   * @param {Object} request.recruiter
   *   Required.
   *
   *   The recruiter to be created.
   *
   *   This object should have the same structure as [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error, ?Object)} [callback]
   *   The function which will be called with the result of the API call.
   *
   *   The second parameter to the callback is an object representing [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is an object representing [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.RecruiterServiceClient({
   *   // optional auth parameters.
   * });
   *
   * const formattedParent = client.tenantPath('[PROJECT]', '[TENANT]');
   * const recruiter = {};
   * const request = {
   *   parent: formattedParent,
   *   recruiter: recruiter,
   * };
   * client.createRecruiter(request)
   *   .then(responses => {
   *     const response = responses[0];
   *     // doThingsWith(response)
   *   })
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  createRecruiter(request, options, callback) {
    if (options instanceof Function && callback === undefined) {
      callback = options;
      options = {};
    }
    options = options || {};
    options.otherArgs = options.otherArgs || {};
    options.otherArgs.headers = options.otherArgs.headers || {};
    options.otherArgs.headers['x-goog-request-params'] =
      gax.routingHeader.fromParams({
        'parent': request.parent
      });

    return this._innerApiCalls.createRecruiter(request, options, callback);
  }

  /**
   * Gets the specified recruiter.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} request.name
   *   Required.
   *
   *   Resource name of the recruiter to get.
   *
   *   The format is
   *   "projects/{project_id}/recruiters/{recruiter_id}",
   *   for example, "projects/api-test-project/recruiters/bar".
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error, ?Object)} [callback]
   *   The function which will be called with the result of the API call.
   *
   *   The second parameter to the callback is an object representing [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is an object representing [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.RecruiterServiceClient({
   *   // optional auth parameters.
   * });
   *
   * const formattedName = client.recruiterPath('[PROJECT]', '[TENANT]', '[RECRUITER]');
   * client.getRecruiter({name: formattedName})
   *   .then(responses => {
   *     const response = responses[0];
   *     // doThingsWith(response)
   *   })
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  getRecruiter(request, options, callback) {
    if (options instanceof Function && callback === undefined) {
      callback = options;
      options = {};
    }
    options = options || {};
    options.otherArgs = options.otherArgs || {};
    options.otherArgs.headers = options.otherArgs.headers || {};
    options.otherArgs.headers['x-goog-request-params'] =
      gax.routingHeader.fromParams({
        'name': request.name
      });

    return this._innerApiCalls.getRecruiter(request, options, callback);
  }

  /**
   * Updates the specified recruiter and returns the updated result.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {Object} request.recruiter
   *   Required.
   *
   *   Recruiter to be updated.
   *
   *   This object should have the same structure as [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}
   * @param {Object} [request.updateMask]
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
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error, ?Object)} [callback]
   *   The function which will be called with the result of the API call.
   *
   *   The second parameter to the callback is an object representing [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is an object representing [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.RecruiterServiceClient({
   *   // optional auth parameters.
   * });
   *
   * const recruiter = {};
   * client.updateRecruiter({recruiter: recruiter})
   *   .then(responses => {
   *     const response = responses[0];
   *     // doThingsWith(response)
   *   })
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  updateRecruiter(request, options, callback) {
    if (options instanceof Function && callback === undefined) {
      callback = options;
      options = {};
    }
    options = options || {};
    options.otherArgs = options.otherArgs || {};
    options.otherArgs.headers = options.otherArgs.headers || {};
    options.otherArgs.headers['x-goog-request-params'] =
      gax.routingHeader.fromParams({
        'recruiter.name': request.recruiter.name
      });

    return this._innerApiCalls.updateRecruiter(request, options, callback);
  }

  /**
   * Deletes the specified recruiter.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} request.name
   *   Required.
   *
   *   Resource name of the recruiter to be deleted.
   *
   *   The format is
   *   "projects/{project_id}/recruiters/{recruiter_id}",
   *   for example, "projects/api-test-project/recruiters/bar".
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error)} [callback]
   *   The function which will be called with the result of the API call.
   * @returns {Promise} - The promise which resolves when API call finishes.
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.RecruiterServiceClient({
   *   // optional auth parameters.
   * });
   *
   * const formattedName = client.recruiterPath('[PROJECT]', '[TENANT]', '[RECRUITER]');
   * client.deleteRecruiter({name: formattedName}).catch(err => {
   *   console.error(err);
   * });
   */
  deleteRecruiter(request, options, callback) {
    if (options instanceof Function && callback === undefined) {
      callback = options;
      options = {};
    }
    options = options || {};
    options.otherArgs = options.otherArgs || {};
    options.otherArgs.headers = options.otherArgs.headers || {};
    options.otherArgs.headers['x-goog-request-params'] =
      gax.routingHeader.fromParams({
        'name': request.name
      });

    return this._innerApiCalls.deleteRecruiter(request, options, callback);
  }

  /**
   * Add profile tags into a recruiter.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} request.recruiter
   *   Required.
   *
   *   Resource name assigned to a recruiter by the API.
   *
   *   The format is
   *   "projects/{project_id}}/recruiters/{recruiter_id}",
   *   for example, "projects/api-test-project/recruiters/bar".
   * @param {string[]} request.profileTags
   *   Required.
   *
   *   The profile tags added to the recruiter's list of profile tags.
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error, ?Object)} [callback]
   *   The function which will be called with the result of the API call.
   *
   *   The second parameter to the callback is an object representing [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is an object representing [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.RecruiterServiceClient({
   *   // optional auth parameters.
   * });
   *
   * const formattedRecruiter = client.recruiterPath('[PROJECT]', '[TENANT]', '[RECRUITER]');
   * const profileTags = [];
   * const request = {
   *   recruiter: formattedRecruiter,
   *   profileTags: profileTags,
   * };
   * client.addRecruiterProfileTags(request)
   *   .then(responses => {
   *     const response = responses[0];
   *     // doThingsWith(response)
   *   })
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  addRecruiterProfileTags(request, options, callback) {
    if (options instanceof Function && callback === undefined) {
      callback = options;
      options = {};
    }
    options = options || {};
    options.otherArgs = options.otherArgs || {};
    options.otherArgs.headers = options.otherArgs.headers || {};
    options.otherArgs.headers['x-goog-request-params'] =
      gax.routingHeader.fromParams({
        'recruiter': request.recruiter
      });

    return this._innerApiCalls.addRecruiterProfileTags(request, options, callback);
  }

  /**
   * Remove profile tags from a recruiter.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} request.recruiter
   *   Required.
   *
   *   Resource name assigned to a recruiter by the API.
   *
   *   The format is
   *   "projects/{project_id}/recruiters/{recruiter_id}",
   *   for example, "projects/api-test-project/recruiters/bar".
   * @param {string[]} request.profileTags
   *   Required.
   *
   *   The profile tags removed from the recruiter's list of profile tags.
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error, ?Object)} [callback]
   *   The function which will be called with the result of the API call.
   *
   *   The second parameter to the callback is an object representing [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is an object representing [Recruiter]{@link google.cloud.talent.v4beta1.Recruiter}.
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.RecruiterServiceClient({
   *   // optional auth parameters.
   * });
   *
   * const formattedRecruiter = client.recruiterPath('[PROJECT]', '[TENANT]', '[RECRUITER]');
   * const profileTags = [];
   * const request = {
   *   recruiter: formattedRecruiter,
   *   profileTags: profileTags,
   * };
   * client.removeRecruiterProfileTags(request)
   *   .then(responses => {
   *     const response = responses[0];
   *     // doThingsWith(response)
   *   })
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  removeRecruiterProfileTags(request, options, callback) {
    if (options instanceof Function && callback === undefined) {
      callback = options;
      options = {};
    }
    options = options || {};
    options.otherArgs = options.otherArgs || {};
    options.otherArgs.headers = options.otherArgs.headers || {};
    options.otherArgs.headers['x-goog-request-params'] =
      gax.routingHeader.fromParams({
        'recruiter': request.recruiter
      });

    return this._innerApiCalls.removeRecruiterProfileTags(request, options, callback);
  }

  // --------------------
  // -- Path templates --
  // --------------------

  /**
   * Return a fully-qualified tenant resource name string.
   *
   * @param {String} project
   * @param {String} tenant
   * @returns {String}
   */
  tenantPath(project, tenant) {
    return this._pathTemplates.tenantPathTemplate.render({
      project: project,
      tenant: tenant,
    });
  }

  /**
   * Return a fully-qualified recruiter resource name string.
   *
   * @param {String} project
   * @param {String} tenant
   * @param {String} recruiter
   * @returns {String}
   */
  recruiterPath(project, tenant, recruiter) {
    return this._pathTemplates.recruiterPathTemplate.render({
      project: project,
      tenant: tenant,
      recruiter: recruiter,
    });
  }

  /**
   * Parse the tenantName from a tenant resource.
   *
   * @param {String} tenantName
   *   A fully-qualified path representing a tenant resources.
   * @returns {String} - A string representing the project.
   */
  matchProjectFromTenantName(tenantName) {
    return this._pathTemplates.tenantPathTemplate
      .match(tenantName)
      .project;
  }

  /**
   * Parse the tenantName from a tenant resource.
   *
   * @param {String} tenantName
   *   A fully-qualified path representing a tenant resources.
   * @returns {String} - A string representing the tenant.
   */
  matchTenantFromTenantName(tenantName) {
    return this._pathTemplates.tenantPathTemplate
      .match(tenantName)
      .tenant;
  }

  /**
   * Parse the recruiterName from a recruiter resource.
   *
   * @param {String} recruiterName
   *   A fully-qualified path representing a recruiter resources.
   * @returns {String} - A string representing the project.
   */
  matchProjectFromRecruiterName(recruiterName) {
    return this._pathTemplates.recruiterPathTemplate
      .match(recruiterName)
      .project;
  }

  /**
   * Parse the recruiterName from a recruiter resource.
   *
   * @param {String} recruiterName
   *   A fully-qualified path representing a recruiter resources.
   * @returns {String} - A string representing the tenant.
   */
  matchTenantFromRecruiterName(recruiterName) {
    return this._pathTemplates.recruiterPathTemplate
      .match(recruiterName)
      .tenant;
  }

  /**
   * Parse the recruiterName from a recruiter resource.
   *
   * @param {String} recruiterName
   *   A fully-qualified path representing a recruiter resources.
   * @returns {String} - A string representing the recruiter.
   */
  matchRecruiterFromRecruiterName(recruiterName) {
    return this._pathTemplates.recruiterPathTemplate
      .match(recruiterName)
      .recruiter;
  }
}


module.exports = RecruiterServiceClient;
