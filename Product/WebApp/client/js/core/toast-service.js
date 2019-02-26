var ToastService = (function () {
    var _defaultToastOptions = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": false,
        "progressBar": true,
        "positionClass": "toast-bottom-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": 500,
        "hideDuration": 500,
        "timeOut": 7000,
        "extendedTimeOut": 3000,
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut",
        "tapToDismiss": false
    };

    var success = function (message, title = "Success", options = {}) {
        var settings = $.extend({}, _defaultToastOptions, options);
        toastr.success(message, title, settings);
    };

    var error = function (message, title = "Error", options = {}) {
        var settings = $.extend({}, _defaultToastOptions, options);
        toastr.error(message, title, settings);
    }

    var info = function (message, title = "Info", options = {}) {
        var settings = $.extend({}, _defaultToastOptions, options);
        toastr.info(message, title, settings);
    }

    var warning = function (message, title = "Warning", options = {}) {
        var settings = $.extend({}, _defaultToastOptions, options);
        toastr.warning(message, title, settings);
    }

    return {
        success: success,
        error: error,
        warning: warning,
        info: info
    };
})();