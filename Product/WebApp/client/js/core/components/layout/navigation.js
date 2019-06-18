var setNotificationCount = function () {
    if (userNotificationCount > 0) {
        $(".nav-notifications .notification-count").addClass("has-notifications");
        if (userNotificationCount > 99)
            userNotificationCount = "99+";
        $(".nav-notifications .notification-count").html(userNotificationCount);
    }
    else {
        $(".nav-notifications .notification-count").html("");
        $(".nav-notifications .notification-count").removeClass("has-notifications");
    }
};

$(document).ready(function () {
    $(".nav-notifications").append("<div class=\"notification-count\"></div>");
    if (userNotificationCount !== undefined) {
        setNotificationCount(userNotificationCount);
    }
});
