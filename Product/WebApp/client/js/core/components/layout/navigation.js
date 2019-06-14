$(document).ready(function () {
    if (userNotificationCount > 0) {
        var currentAnchor = $(".nav-notifications").html();
        if (userNotificationCount > 99)
            userNotificationCount = "99+";
        $(".nav-notifications").html(currentAnchor + "<div class=\"notification-count\"><strong>" + userNotificationCount + "</strong></div>");
    }
});
