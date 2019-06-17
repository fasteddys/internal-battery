var CareerCircleWebApp = (function () {

    // setup axios instance which we will use for http requests to API
    var _http = axios.create({
        headers: {
            'Content-Type': 'application/json'
        }
    });

    var updateSubscriberNotification = async function (notification) {
        var formData = new FormData();
        formData.append("notification", notification);
        return _http.post('/Dashboard/SubscriberHasReadNotification', JSON.stringify(notification), {
            headers: {
                'Content-Type': 'application/json'
            }
        });

    }

    return {
        updateSubscriberNotification: updateSubscriberNotification
    };

})();