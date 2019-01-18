
var CareerCircleSignalR = (function (hubUrl) {
    var _session_key = "ccsignalr";
    // Important! Must match Constants.SignalR.CookieKey in UpdiddyLib constants
    var _cookie_key = "ccsignalr_connection_id";
    var _connection = null;    
    var _signalr_url = hubUrl;
    var _subscriberGuid = null;
    
    
    var connectGood = function () {
        _connection = new signalR.HubConnectionBuilder()
            .withUrl(_signalr_url)
            .build();

        _connection.start()
            .then(setHubId)
            .catch(err => console.error(err.toString()));         
    }

    var connect = async (subscriberGuid) => {

        _subscriberGuid = subscriberGuid;
        _connection = new signalR.HubConnectionBuilder()
            .withUrl(_signalr_url)
            .build();
        await _connection.start()
            .then(await setHubId)
            .catch(err => console.error(err.toString()));     
    }
  
    var setHubId = async () => {
        await _connection.invoke('getConnectionId')           
            .then(function (connectionId) {
                // Cache the connection id in browser session
                SessionStorage.set(_session_key, connectionId);
                // Expire existing cookie 
                document.cookie = _cookie_key + "; expires=Thu, 01 Jan 1970 00:00:01 GMT; path=/";
                // Set new cookie
                document.cookie = _cookie_key + "=" + connectionId + "; path=/";
            });
    }


    var getHubIdTODORemove = async () => {
        return SessionStorage.get(_session_key);
    }

    var getHubId = function () {    
        return SessionStorage.get(_session_key);
    }

    var listen = async (verb, cb) => {
        _connection.on(verb, cb);
        // Inform api which verb the connection is listening for 
        await _connection.invoke('subscribe', _subscriberGuid, verb);    

    }


    var listenOld = function (verb, cb) {
        _connection.on(verb, cb); 
    }

    return {
        getHubId, getHubId,
        listen, listen,
        connect: connect,
        getHubId: getHubId
    };



})(SIGNALR_URL);


 
