


$("#CreatePartnerForm").on("submit", function (e) {
    
    if (!$(this).find("#Name").val() || !$(this).find("#Description").val()) {
        e.preventDefault();
        ToastService.error('Please enter information for both Name and Description.', 'Oops, Something went wrong.');
    }
});

$("#ModifyPartnerForm").on("submit", function (e) {
    
    if (!$(this).find("#Name").val() || !$(this).find("#Description").val()) {
        e.preventDefault();
        ToastService.error('Please enter information for both Name and Description. To delete, return to Partners list and delete partner.', 'Oops, Something went wrong.');
    }
});

$("#DeletePartnerConfirmationModal .btn-danger").on("click", function () {
    window.location.replace("/admin/deletepartner/" + $("#PartnerToDelete").val());
});


$(document).on("click", "a.partner-delete", function () {
    var partner = $(this).data("partner");
    var partnerName = $(this).data("partner-name");
    $("#PartnerToDelete").val(partner);
    $("#DeletePartnerConfirmationModal #DeleteConfirmationPartnerName").html(partnerName);
    $("#DeletePartnerConfirmationModal").modal();
});