


 

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

$("#IsTargeted").on("click", function(){
    
    var divToShow = $(".group-notification-select");
    divToShow.toggleClass("hidden");
});

$( document ).ready(function() {
    $( "#IsTargeted" ).prop( "checked", false );
});