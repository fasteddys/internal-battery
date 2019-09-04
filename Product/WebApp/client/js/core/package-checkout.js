$( document ).ready(function() {
    $("#PackageAgreeToTermsAndConditionsCheckbox").prop("checked", false);
    

    $('#PackageAgreeToTermsAndConditionsCheckbox').change(function() {
        $("#PackageAgreeToTermsAndConditions").val(this.checked);
        if(this.checked) {
            $(".authenticated-section input").prop( "disabled", false );
            $(".authenticated-section select").prop( "disabled", false );
            $("#PackageCheckout").prop( "disabled", false );
        }
        else{
            $(".authenticated-section input").prop( "disabled", true );
            $(".authenticated-section select").prop( "disabled", true );
            $("#PackageCheckout").prop( "disabled", true );
        }     
    });
    
});

var calculatePackagePrice = function(){
    var InitialPackagePrice = $("#InitialPackagePrice").html();
    var PromoCodeTotal = $("#PromoCodeTotal").html();
    return InitialPackagePrice - PromoCodeTotal;
}

var submitPackagePayment = function(){
    $.ajax({
        type: 'POST',
        url: $("#PackageCheckoutForm").attr('action'),
        data: $("#PackageCheckoutForm").serialize(),
        success: function (data) {
            switch(data.statusCode){
                case 200:
                    document.location.href = "/career-services/" + pageSlug + "/confirmation";
                    break;
                case 400:
                    ToastService.error("The information you've supplied is incorrect. Please fix and submit again");
                    break;
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            ToastService.error("Something went wrong with your checkout.");
        }
    });
}