var SessionStorage = (function () {
    return {
        set: function (key, value) {
            window.sessionStorage.setItem(key, value);
        },
        get: function (key) {
            return JSON.parse(window.sessionStorage.getItem(key));
        }
    };
})();