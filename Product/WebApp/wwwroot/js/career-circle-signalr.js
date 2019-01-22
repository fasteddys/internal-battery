
var CareerCircleSignalR = (function (hubUrl) {
    var _session_key = "ccsignalr";
    // Important! Must match Constants.SignalR.CookieKey in UpdiddyLib constants
    var _cookie_key = "ccsignalr_connection_id";
    var _connection = null;    
    var _signalr_url = hubUrl;
    var _subscriberGuid = null;
    
    
    var connectGood = function () {
        console.log("signalR connectGood: Opening connection...");
        _connection = new signalR.HubConnectionBuilder()
            .withUrl(_signalr_url)
            .build();
        console.log("signalR connectGood: Connection made.");
        _connection.start()
            .then(setHubId)
            .catch(err => console.error(err.toString()));      
        console.log("signalR connectGood: Connection started.");
    }

    var connect = async (subscriberGuid) => {

        _subscriberGuid = subscriberGuid;
        _connection = new signalR.HubConnectionBuilder()
            .withUrl(_signalr_url)
            .build();
        console.log("signalR connect: Connection made.");
        await _connection.start()
            .then(await setHubId)
            .catch(err => console.error(err.toString()));     
        console.log("signalR connect: Connection started.");
    }
  
    var setHubId = async () => {
        console.log("signalR setHubId: start");
        await _connection.invoke('getConnectionId')           
            .then(function (connectionId) {
                // Cache the connection id in browser session
                console.log("signalR setHubId: setting sessionStorage");
                SessionStorage.set(_session_key, connectionId);
                // Expire existing cookie 
                console.log("signalR setHubId: setting cookie1");
                document.cookie = _cookie_key + "; expires=Thu, 01 Jan 1970 00:00:01 GMT; path=/";
                // Set new cookie
                console.log("signalR setHubId: setting cookie2");
                document.cookie = _cookie_key + "=" + connectionId + "; path=/";
            });
    }


    var getHubIdTODORemove = async () => {
        console.log("signalR getHubIdTODORemove: start");
        return SessionStorage.get(_session_key);
    }

    var getHubId = function () {    
        console.log("signalR getHubId: start");
        return SessionStorage.get(_session_key);
    }

    var listen = async (verb, cb) => {
        console.log("signalR listen: Listening...");
        _connection.on(verb, cb);
        // Inform api which verb the connection is listening for 
        await _connection.invoke('subscribe', _subscriberGuid, verb);    

    }


    var listenOld = function (verb, cb) {
        console.log("signalR listenOld: Listening...");
        _connection.on(verb, cb); 
    }

    return {
        getHubId, getHubId,
        listen, listen,
        connect: connect,
        getHubId: getHubId
    };



})(SIGNALR_URL);


 
