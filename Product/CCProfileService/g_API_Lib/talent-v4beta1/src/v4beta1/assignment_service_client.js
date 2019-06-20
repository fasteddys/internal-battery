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

const gapicConfig = require('./assignment_service_client_config.json');
const gax = require('google-gax');
const merge = require('lodash.merge');
const path = require('path');

const VERSION = require('../../package.json').version;

/**
 * A service that handles assignment management, including CRUD and enumeration.
 *
 * @class
 * @memberof v4beta1
 */
class AssignmentServiceClient {
  /**
   * Construct an instance of AssignmentServiceClient.
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
        'google/cloud/talent/v4beta1/assignment_service.proto'
      )
    );

    // Some of the methods on this service return "paged" results,
    // (e.g. 50 results at a time, with tokens to get subsequent
    // pages). Denote the keys used for pagination and results.
    this._descriptors.page = {
      listAssignments: new gax.PageDescriptor(
        'pageToken',
        'nextPageToken',
        'assignments'
      ),
    };

    // Put together the default options sent with requests.
    const defaults = gaxGrpc.constructSettings(
      'google.cloud.talent.v4beta1.AssignmentService',
      gapicConfig,
      opts.clientConfig,
      {'x-goog-api-client': clientHeader.join(' ')}
    );

    // Set up a dictionary of "inner API calls"; the core implementation
    // of calling the API is handled in `google-gax`, with this code
    // merely providing the destination and request information.
    this._innerApiCalls = {};

    // Put together the "service stub" for
    // google.cloud.talent.v4beta1.AssignmentService.
    const assignmentServiceStub = gaxGrpc.createStub(
      protos.google.cloud.talent.v4beta1.AssignmentService,
      opts
    );

    // Iterate over each of the methods that the service provides
    // and create an API call method for each.
    const assignmentServiceStubMethods = [
      'createAssignment',
      'getAssignment',
      'updateAssignment',
      'deleteAssignment',
      'listAssignments',
    ];
    for (const methodName of assignmentServiceStubMethods) {
      this._innerApiCalls[methodName] = gax.createApiCall(
        assignmentServiceStub.then(
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
   * Creates a new assignment entity.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} [request.parent]
   *   Required.
   *
   *   Resource name of the project under which the assignment is created.
   *
   *   The format is
   *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}", for
   *   example, "projects/test-project/tenants/test-tenant/profiles/test-profile".
   * @param {Object} [request.assignment]
   *   Required.
   *
   *   The assignment to be created.
   *
   *   This object should have the same structure as [Assignment]{@link google.cloud.talent.v4beta1.Assignment}
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error, ?Object)} [callback]
   *   The function which will be called with the result of the API call.
   *
   *   The second parameter to the callback is an object representing [Assignment]{@link google.cloud.talent.v4beta1.Assignment}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is an object representing [Assignment]{@link google.cloud.talent.v4beta1.Assignment}.
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.AssignmentServiceClient({
   *   // optional auth parameters.
   * });
   *
   *
   * client.createAssignment({})
   *   .then(responses => {
   *     const response = responses[0];
   *     // doThingsWith(response)
   *   })
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  createAssignment(request, options, callback) {
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

    return this._innerApiCalls.createAssignment(request, options, callback);
  }

  /**
   * Retrieves specified assignment.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} [request.name]
   *   Required.
   *
   *   The resource name of the assignment to be retrieved.
   *
   *   The format is
   *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}/assignments/{assignment_id}",
   *   for example,
   *   "projects/test-project/tenants/test-tenant/profiles/test-profile/assignments/test-assignment".
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error, ?Object)} [callback]
   *   The function which will be called with the result of the API call.
   *
   *   The second parameter to the callback is an object representing [Assignment]{@link google.cloud.talent.v4beta1.Assignment}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is an object representing [Assignment]{@link google.cloud.talent.v4beta1.Assignment}.
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.AssignmentServiceClient({
   *   // optional auth parameters.
   * });
   *
   *
   * client.getAssignment({})
   *   .then(responses => {
   *     const response = responses[0];
   *     // doThingsWith(response)
   *   })
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  getAssignment(request, options, callback) {
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

    return this._innerApiCalls.getAssignment(request, options, callback);
  }

  /**
   * Updates specified assignment.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {Object} [request.assignment]
   *   Required.
   *
   *   The assignment resource to replace the current resource in the system.
   *
   *   This object should have the same structure as [Assignment]{@link google.cloud.talent.v4beta1.Assignment}
   * @param {Object} [request.updateMask]
   *   Optional but strongly recommended for the best service
   *   experience.
   *
   *   If update_mask is provided, only the specified fields in
   *   assignment are updated. Otherwise all the fields are updated.
   *
   *   A field mask to specify the assignment fields to be updated. Only
   *   top level fields of Assignment are supported.
   *
   *   This object should have the same structure as [FieldMask]{@link google.protobuf.FieldMask}
   * @param {Object} [options]
   *   Optional parameters. You can override the default settings for this call, e.g, timeout,
   *   retries, paginations, etc. See [gax.CallOptions]{@link https://googleapis.github.io/gax-nodejs/global.html#CallOptions} for the details.
   * @param {function(?Error, ?Object)} [callback]
   *   The function which will be called with the result of the API call.
   *
   *   The second parameter to the callback is an object representing [Assignment]{@link google.cloud.talent.v4beta1.Assignment}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is an object representing [Assignment]{@link google.cloud.talent.v4beta1.Assignment}.
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.AssignmentServiceClient({
   *   // optional auth parameters.
   * });
   *
   *
   * client.updateAssignment({})
   *   .then(responses => {
   *     const response = responses[0];
   *     // doThingsWith(response)
   *   })
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  updateAssignment(request, options, callback) {
    if (options instanceof Function && callback === undefined) {
      callback = options;
      options = {};
    }
    options = options || {};
    options.otherArgs = options.otherArgs || {};
    options.otherArgs.headers = options.otherArgs.headers || {};
    options.otherArgs.headers['x-goog-request-params'] =
      gax.routingHeader.fromParams({
        'assignment.name': request.assignment.name
      });

    return this._innerApiCalls.updateAssignment(request, options, callback);
  }

  /**
   * Deletes specified assignment.
   * Prerequisite: The assignment has no jobs associated with it.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} [request.name]
   *   Required.
   *
   *   The resource name of the assignment to be deleted.
   *
   *   The format is
   *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}/assignments/{assignment_id}",
   *   for example,
   *   "projects/test-project/tenants/test-tenant/profiles/test-profile/assignments/test-assignment".
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
   * const client = new talent.v4beta1.AssignmentServiceClient({
   *   // optional auth parameters.
   * });
   *
   *
   * client.deleteAssignment({}).catch(err => {
   *   console.error(err);
   * });
   */
  deleteAssignment(request, options, callback) {
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

    return this._innerApiCalls.deleteAssignment(request, options, callback);
  }

  /**
   * Lists all assignments associated with the profile.
   *
   * @param {Object} request
   *   The request object that will be sent.
   * @param {string} [request.parent]
   *   Required.
   *
   *   Resource name of the project under which the assignment is created.
   *
   *   The format is
   *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}", for
   *   example, "projects/test-project/tenants/test-tenant/profiles/test-profile".
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
   *   The second parameter to the callback is Array of [Assignment]{@link google.cloud.talent.v4beta1.Assignment}.
   *
   *   When autoPaginate: false is specified through options, it contains the result
   *   in a single response. If the response indicates the next page exists, the third
   *   parameter is set to be used for the next request object. The fourth parameter keeps
   *   the raw response object of an object representing [ListAssignmentsResponse]{@link google.cloud.talent.v4beta1.ListAssignmentsResponse}.
   * @returns {Promise} - The promise which resolves to an array.
   *   The first element of the array is Array of [Assignment]{@link google.cloud.talent.v4beta1.Assignment}.
   *
   *   When autoPaginate: false is specified through options, the array has three elements.
   *   The first element is Array of [Assignment]{@link google.cloud.talent.v4beta1.Assignment} in a single response.
   *   The second element is the next request object if the response
   *   indicates the next page exists, or null. The third element is
   *   an object representing [ListAssignmentsResponse]{@link google.cloud.talent.v4beta1.ListAssignmentsResponse}.
   *
   *   The promise has a method named "cancel" which cancels the ongoing API call.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.AssignmentServiceClient({
   *   // optional auth parameters.
   * });
   *
   * // Iterate over all elements.
   * client.listAssignments({})
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
   *     return client.listAssignments(nextRequest, options).then(callback);
   *   }
   * }
   * client.listAssignments({}, options)
   *   .then(callback)
   *   .catch(err => {
   *     console.error(err);
   *   });
   */
  listAssignments(request, options, callback) {
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

    return this._innerApiCalls.listAssignments(request, options, callback);
  }

  /**
   * Equivalent to {@link listAssignments}, but returns a NodeJS Stream object.
   *
   * This fetches the paged responses for {@link listAssignments} continuously
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
   * @param {string} [request.parent]
   *   Required.
   *
   *   Resource name of the project under which the assignment is created.
   *
   *   The format is
   *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}", for
   *   example, "projects/test-project/tenants/test-tenant/profiles/test-profile".
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
   *   An object stream which emits an object representing [Assignment]{@link google.cloud.talent.v4beta1.Assignment} on 'data' event.
   *
   * @example
   *
   * const talent = require('@google-cloud/talent');
   *
   * const client = new talent.v4beta1.AssignmentServiceClient({
   *   // optional auth parameters.
   * });
   *
   *
   * client.listAssignmentsStream({})
   *   .on('data', element => {
   *     // doThingsWith(element)
   *   }).on('error', err => {
   *     console.log(err);
   *   });
   */
  listAssignmentsStream(request, options) {
    options = options || {};

    return this._descriptors.page.listAssignments.createStream(
      this._innerApiCalls.listAssignments,
      request,
      options
    );
  };
}


module.exports = AssignmentServiceClient;
