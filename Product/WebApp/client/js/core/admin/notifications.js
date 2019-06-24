


$("#CreateNotificationForm").on("submit", function (e) {

    if (!$(this).find("#Title").val() || !$(this).find("#Description").val()) {
        e.preventDefault();
        ToastService.error('Please enter information for both Title and Description.', 'Oops, Something went wrong.');
    }
});

$("#ModifyNotificationForm").on("submit", function (e) {

    if (!$(this).find("#Title").val() || !$(this).find("#Description").val()) {
        e.preventDefault();
        ToastService.error('Please enter information for both Title and Description. To delete, return to Partners list and delete partner.', 'Oops, Something went wrong.');
    }
});

$("#DeleteNotificationConfirmationModal .btn-danger").on("click", function () {
    window.location.replace("/admin/deletenotification/" + $("#NotificationToDelete").val());
});


$(document).on("click", "a.notification-delete", function () {
    var notification = $(this).data("notification");
    var notificationName = $(this).data("notification-name");
    $("#NotificationToDelete").val(notification);
    $("#DeleteNotificationConfirmationModal #DeleteConfirmationNotificationName").html(notificationName);
    $("#DeleteNotificationConfirmationModal").modal();
});