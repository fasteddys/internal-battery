$(document).ready(function () {
    if (userNotificationCount > 0) {
        if (userNotificationCount > 99)
            userNotificationCount = "99+";
        $(".nav-notifications").append("<div class=\"notification-count\"><strong>" + userNotificationCount + "</strong></div>");
    }
});
