// Write your Javascript code.
$(document).ready(function () {
    $("#SameAsAboveCheckbox").change(function () {
        if (this.checked) {
            $('.billing-info-container').hide();
        }
        else {
            $('.billing-info-container').show();
        }
    });

    $("#TermsOfServiceCheckbox").change(function () {
        if (this.checked) {
            $('#EnrollmentSubmitButton').prop('disabled', false);
        }
        else {
            $('#EnrollmentSubmitButton').prop('disabled', true);
        }
    });
});


