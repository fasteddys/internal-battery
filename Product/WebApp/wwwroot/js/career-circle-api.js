// todo: maybe utilize DI SessionStorage
var CareerCircleAPI = (function (apiUrl) {
    var _session_key = "ccapi";
    var _api_url = apiUrl.trim().replace(/\/$/, "");

    var authorizedRequest = function (endpoint) {
        var jwt;
        return getToken().then(function (data) {
            jwt = data
            return $.ajax({
                url: _api_url + endpoint,
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + jwt.AccessToken
                }
            })
        });
    };

    // checks token to see if it is expired, if expired then get another one
    var getToken = function () {
        var jwt = SessionStorage.get(_session_key);

        if (jwt == null || new Date() <= new Date(jwt.ExpiresOn)) {
            return new Promise(resolve => {
                retrieveToken().done(function (data) {
                    SessionStorage.set(_session_key, data);
                    resolve(SessionStorage.get(_session_key));
                });
            });
        };

        return new Promise(resolve => {
            resolve(jwt);
        });
    };

    var signOut = function () {
        SessionStorage.clear();
    };

    // call to webapp to get token
    var retrieveToken = function () {
        return $.ajax({
            url: '/session/token',
            headers: {
                'Content-Type': 'application/json'
            },
        });
    };

    // call to profile endpoint
    var getProfile = function () {
        return authorizedRequest("/profile")
    };

    return {
        getProfile: getProfile,
        signOut: signOut
    };
})(API_URL);