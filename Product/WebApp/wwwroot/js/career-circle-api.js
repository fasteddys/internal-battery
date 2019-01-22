// todo: maybe utilize DI SessionStorage
var CareerCircleAPI = (function (apiUrl) {
    var _session_key = "ccapi";
    var _api_url = apiUrl.trim().replace(/\/$/, "");

    // setup axios instance which we will use for http requests to API
    var _http = axios.create({
        baseURL: apiUrl,

        transformRequest: [function (data, headers) {
            console.log('transforming request');
            headers['Authorization'] = 'Bearer ' + SessionStorage.getJSON(_session_key).AccessToken;
            return data;
        }],
        headers: {
            'Content-Type': 'application/json'
        }
    });

    // on requests check token and set it
    _http.interceptors.request.use(
        config => {
            return new Promise((resolve, reject) => {
                getToken().then(function (jwt) {
                    resolve(config);
                })
            })
        }
    );

    // checks token to see if it is expired, if expired then get another one
    var getToken = function () {
        var jwt = SessionStorage.get(_session_key);

        if (jwt == null || new Date() <= new Date(jwt.ExpiresOn)) {
            return new Promise(resolve => {
                retrieveToken().done(function (response) {
                    SessionStorage.set(_session_key, JSON.stringify(response.data));
                    resolve(SessionStorage.getJSON(_session_key));
                });
            });
        };

        return new Promise(resolve => {
            resolve(JSON.parse(jwt));
        });
    };

    var signOut = function () {
        SessionStorage.clear();
    };

    // call to webapp to get token
    var retrieveToken = function () {
        return axios.get('/session/token');
    };

    // call to profile endpoint
    var getProfile = function () {
        return _http.get('/profile');
    };

    var uploadResume = function (resume, parseResume) {
        var formData = new FormData();
        formData.append("resume", resume);

        if (parseResume == null)
            parseResume = false;

        formData.append("parseResume", parseResume);

        return _http.post('/resume/upload', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
    }

    var deleteFile = function (fileId) {
        return new Promise(resolve => {
            getToken().then(function (jwt) {
                var path = `/subscriber/${jwt.UniqueId}/file/${fileId}`;
                resolve(_http.delete(path));
            });
        });
    }

    return {
        getProfile: getProfile,
        uploadResume: uploadResume,
        deleteFile: deleteFile,
        signOut: signOut
    };
})(API_URL);