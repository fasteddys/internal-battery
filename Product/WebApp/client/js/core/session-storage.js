var SessionStorage = (function () {
    return {
        set: function (key, value) {
            window.sessionStorage.setItem(key, value);
        },
        getJSON: function (key) {
            return JSON.parse(window.sessionStorage.getItem(key));
        },
        get: function (key) {
            return window.sessionStorage.getItem(key);
        },
        clear: function () {
            window.sessionStorage.clear();
        }
    };
})();