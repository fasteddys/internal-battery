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

    return {
        subscriberReadNotification: subscriberReadNotification
    };

})();