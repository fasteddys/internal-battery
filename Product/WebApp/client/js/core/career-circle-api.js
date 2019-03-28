// todo: maybe utilize DI SessionStorage
var CareerCircleAPI = (function (apiUrl) {
    var _session_key = "ccapi";
    var _api_url = apiUrl.trim().replace(/\/$/, "");

    // setup axios instance which we will use for http requests to API
    var _http = axios.create({
        baseURL: apiUrl,

        transformRequest: [function (data, headers) {
            headers['Authorization'] = 'Bearer ' + SessionStorage.getJSON(_session_key).AccessToken;
            return data;
        }],
        headers: {
            'Content-Type': 'application/json'
        }
    });

    // on requests check token and set it
    _http.interceptors.request.use(
        function(config) {
            return new Promise(function (resolve, reject) {
                getToken().then(function (jwt) {
                    resolve(config);
                })
            })
        }
    );

    // checks token to see if it is expired, if expired then get another one
    var getToken = function () {
        var jwt = SessionStorage.get(_session_key);

        if (jwt == null || new Date() >= new Date(jwt.ExpiresOn)) {
            return new Promise(function(resolve) {
                retrieveToken().done(function (response) {
                    SessionStorage.set(_session_key, JSON.stringify(response.data));
                    resolve(SessionStorage.getJSON(_session_key));
                });
            });
        };

        return new Promise(function(resolve) {
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

    var deleteFile = function (fileId) {
        return new Promise(function(resolve) {
            getToken().then(function (jwt) {
                var path = '/subscriber/' + jwt.UniqueId + '/file/' + fileId;
                resolve(_http.delete(path));
            });
        });
    }

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

    var getContacts = async function(page, pageSize, sorted, filtered, startDate, endDate, partner) {
        var pageIndex = page <= 0 ? 1 : page;
        var params = "";
        if(sorted && sorted.length > 0) {
            var params = "&sort=";
            for(var i = 0; i < sorted.length; i++) {
                var col = sorted[i];
                var sortType = col.desc ? "desc" : "asc";
                params += encodeURI(col.id + " " + sortType) 
            } 
        }
        if(filtered && filtered.length > 0) {
            for(var i = 0; i < filtered.length; i++) {
                var col = filtered[i];
                params += "&" + encodeURI(col.id) + "=" + encodeURI(col.value);
            }
        }

        if(startDate)
            params += "&startDate=" + moment(startDate).startOf('day').valueOf();

        if(endDate)
            params += "&endDate=" + moment(endDate).endOf('day').valueOf();

        if(partner)
            params += "&partnerId=" + partner;

        return await _http.get('/contact?page=' + pageIndex + '&pageSize=' + pageSize + params);
    }

    var getPartners = async function() {
        return await _http.get('/partners');
    }

    var requestVerification = function (verifyUrl) {
        return _http.post('/subscriber/request-verification', JSON.stringify({ verifyUrl: verifyUrl }));
    }

    return {
        getProfile: getProfile,
        uploadResume: uploadResume,
        deleteFile: deleteFile,
        signOut: signOut,
        getContacts: getContacts,
        getPartners: getPartners,
        requestVerification: requestVerification
    };
})(API_URL);