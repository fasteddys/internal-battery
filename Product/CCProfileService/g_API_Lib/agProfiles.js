module.exports = async function (creds) {
    const { google } = require('googleapis');
    const path = require("path");

    let authCreds; 

   try {
        authCreds = JSON.parse(new Buffer(creds, 'base64').toString('utf8'));
    } catch (error) {
        throw new Error(`Unable to Parse Creds:: ${error}`);
    }

    if (!authCreds) {
        throw new Error('The creds variable must be supplied!');
    } 

    const discoveryDoc = path.resolve(`${path.resolve(__dirname)}/discoveryFiles/20181105-cloud_profile_discovery_v2alpha1_distrib.json`.replace('//', './'));
    let profiles = await google.discoverAPI(discoveryDoc);

    //added this for webpack compatibility
    const requireFunc = typeof __webpack_require__ === "function" ? __non_webpack_require__ : require;

    const jwtClient = google.auth.fromJSON(authCreds); //new google.auth.JWT(keyClientEmail, null, keyPrivateKey, ["https://www.googleapis.com/auth/cloudprofile"], null);
    jwtClient.scopes = ["https://www.googleapis.com/auth/cloudprofile"];
    let tokens;

    module.authenticate = function () {
        return new Promise((resolve, reject) => {
            if (!tokens) {
                jwtClient.authorize(function (err, newToken) {
                    if (err) {
                        reject(err);
                    }
                    tokens = newToken;

                    resolve(jwtClient);
                });
            } else {
                resolve(jwtClient);
            }
        });
    }

    module.parseResumeToProfile = function (base64Document) {

        return new Promise((resolve, reject) => {

            module.authenticate().then((jwt) => {


                let request = {
                    'resume': base64Document
                };

                profiles.resumes.parse({ auth: jwt, resource: request }, function (err, result) {
                    if (err) {
                        console.error(`Failed to parse resumes! ${err}, document length: ${base64Document.length}`);
                        reject(err);
                    }

                    if (result && result.data) {
                        console.log(`Parse Succecded, document length: ${base64Document.length}`)
                        resolve(result.data.profile);
                    } else {
                        reject('Document Unparsable');
                    }

                });

            });
        });


    }

    module.createTenant = function (uniqueTenantId, learningType) {

        return new Promise((resolve, reject) => {

            module.authenticate().then((jwt) => {

                let request = {
                    'externalId': uniqueTenantId,
                    'learningType': learningType || 'ISOLATED'
                };

                profiles.tenants.create({ auth: jwt, resource: request }, function (err, result) {
                    if (err) {
                        console.error('Failed to create tenant! ' + err);
                        reject(err);
                    }

                    if (result && result.data) {
                        resolve(result.data);
                    } else {
                        reject('Profile not created');
                    }


                });

            });
        });


    }


    module.listTenant = function () {

        return new Promise((resolve, reject) => {

            module.authenticate().then((jwt) => {

            

                profiles.tenants.list({ auth: jwt }, function (err, result) {
                    if (err) {
                        console.error('Failed to create tenant! ' + err);
                        reject(err);
                    }

                    if (result && result.data) {
                        console.log('list : ' + result.data);
                        resolve(result.data);
                    } else {
                        reject('Profile not created');
                    }


                });

            });
        });


    }

    module.getTenant = function (tenantName) {


        return new Promise((resolve, reject) => {

            module.authenticate().then((jwt) => {


                profiles.tenants.get({ auth: jwt, name: tenantName }, function (err, result) {
                    if (err) {
                        console.error('Failed to get tenant! ' + err);
                        reject(err);
                    }

                             if (result && result.data) {
                        resolve(result.data);
                    } else {
                        reject('get Tenant Failed');
                    }


                });

            });
        });


    }

    module.getProfile = function (profileName) {


        return new Promise((resolve, reject) => {

            module.authenticate().then((jwt) => {




                profiles.tenants.profiles.get({ auth: jwt, name: profileName }, function (err, result) {
                    if (err) {
                        console.error('Failed to get profile! ' + err);
                        reject(err);
                    }

                    if (result && result.data) {
                        resolve(result.data);
                    } else {
                        reject('get Profile Failed');
                    }


                });

            });
        });


    }


    module.listProfile = function (tenantName, pageSize) {


        return new Promise((resolve, reject) => {

            module.authenticate().then((jwt) => {

             console.log('tenentName: ' + tenantName);
             console.log('pageSize: ' + pageSize);


             let query = {"pageSize": pageSize}

                profiles.tenants.profiles.list({ auth: jwt, parent: tenantName, resource: query  }, function (err, result) {
                    if (err) {
                        console.error('Failed to get profile List! ' + err);
                        reject(err);
                    }

                    if (result && result.data) {
                        resolve(result.data);
                    } else {
                        reject('get Profile List Failed');
                    }


                });

            });
        });


    }

    module.putProfile = function (tenantName, profile) {


        return new Promise((resolve, reject) => {

            module.authenticate().then((jwt) => {

                delete profile.name;
                profiles.tenants.profiles.create({ auth: jwt, parent: tenantName, resource: profile }, function (err, result) {

                    if (err) {
                        console.error('Failed to put profile ' + err);
                        reject(err);
                    }

                    if (result && result.data) {
                        resolve(result.data);
                    } else {
                        reject('create Profile Failed');
                    }


                });

            });
        });


    }

    module.basicQuery = function (tenantName, queryString) {


        return new Promise((resolve, reject) => {

            module.authenticate().then((jwt) => {


                let query = {
                    "requestMetadata": {
                        "userId": "UNKNOWN",
                        "sessionId": "UNKNOWN",
                        "userAgent": "UNKNOWN",
                        "domain": "UNKNOWN",
                    },
                    "profileQuery": {
                        "query": queryString,
                    },
                    "pageSize": 20,
                    "histogramOptions": [
                        {
                            "histogramSpec": 'count(admin1)'
                        },
                        {
                            "histogramSpec": 'count(experience_in_months, [bucket(0, 12, "1 year"), bucket(12, 36, "1-3 years"), bucket(36, MAX, "3+ years")])'
                        }
                    ]
                }

                profiles.tenants.search({ auth: jwt, tenantName: tenantName, resource: query }, function (err, result) {
                    if (err) {
                        console.error('Failed Search ' + err);
                        reject(err);
                    }

                    if (result && result.data) {
                        resolve(result.data);
                    } else {
                        reject('Search Failed');
                    }


                });


            });

        });


    }

    module.query = function (tenantName, requestBody) {


        return new Promise((resolve, reject) => {

            module.authenticate().then((jwt) => {


                profiles.tenants.search({ auth: jwt, tenantName: tenantName, resource: requestBody }, function (err, result) {
                    if (err) {
                        console.error('Failed Search ' + err);
                        reject(err);
                    }

                    if (result && result.data) {
                        resolve(result.data);
                    } else {
                        reject('Search Failed');
                    }


                });


            });

        });


    }


    return module;

}