var CareerCircleWebApp = (function () {

    // setup axios instance which we will use for http requests to API
    var _http = axios.create({
        headers: {
            'Content-Type': 'application/json'
        }
    });

    var subscriberReadNotification = async function (notification) {
        var formData = new FormData();
        formData.append("notification", notification);
        return _http.post('/Dashboard/SubscriberHasReadNotification', JSON.stringify(notification), {
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function (response) {
                userNotificationCount = response.data;
                setNotificationCount();
            })
            .catch(function (error) {
                console.log(error);
            });

    };

    var subscriberDeleteNotification = async function (notification) {
        var formData = new FormData();
        formData.append("notification", notification);
        return _http.post('/Dashboard/DeleteSubscriberNotification', JSON.stringify(notification), {
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function (response) {
                userNotificationCount = response.data;
                setNotificationCount();
            })
            .catch(function (error) {
                console.log(error);
            });
    };

    var toggleNotificationEmails = async function (isNotificationEnabled) {
        var isSuccessful = false;
        var urlValue = isNotificationEnabled ? "true" : "false";
        return _http.get('/Dashboard/ToggleSubscriberNotificationEmail/' + urlValue)
            .then(function (response) {
                if (response.data)
                    isSuccessful = true;
            })
            .catch(function (error) {
                console.log(error);
            });
        return isSuccessful;
    };

    return {
        toggleNotificationEmails: toggleNotificationEmails,
        subscriberReadNotification: subscriberReadNotification,
        subscriberDeleteNotification: subscriberDeleteNotification
    };

})();