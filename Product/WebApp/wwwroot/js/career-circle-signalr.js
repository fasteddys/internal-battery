
var _session_key = "ccsignalr";
var _cookie_key = "ccsignalr_connection_id";
var _signalr_url = 'http://localhost:5002/clienthub/';
var _signalr_api_url = 'http://localhost:5002/api/clienthub/'; 

const connection = new signalR.HubConnectionBuilder()
    .withUrl(_signalr_url)
    .build();

connection.start()
    .then(getHubId)
    .catch(err => console.error(err.toString()));
    
function getHubId()
{
   connection.invoke('getConnectionId')
       .then(function (connectionId) {
           // Cache the connection id in browser session
           SessionStorage.set(_session_key, connectionId);
           // Expire existing cookie 
           document.cookie = _cookie_key + "; expires=Thu, 01 Jan 1970 00:00:01 GMT; path=/";
           // Set new cookie
           document.cookie = _cookie_key + "=" + connectionId + "; path=/";
        });
}
    
connection.on('Send', (message) => {
        alert("SignalR says:" + message);
});


 

    
function AjaxSignalRTest() {

    var hubId = SessionStorage.get(_session_key);
    var url = _signalr_api_url + "test/" + hubId;

    $.ajax({
       url: url
    }).done(function () {
    });

    event.preventDefault();
}


 
