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

    $("#PromoCodeEntered").on('keyup', function (e) {
        e.preventDefault();
        if (e.keyCode === 13) {
            validatePromoCode();
        }
    });
    
});

var calculatePackagePrice = function(){
    var InitialPackagePrice = $("#InitialPackagePrice").html();
    var PromoCodeTotal = $("#PromoCodeTotal").html();
    return (InitialPackagePrice - PromoCodeTotal).toFixed(2);
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

var validatePromoCode = function(){
    $.ajax({
        type: 'POST',
        url: '/services/promo-code/validate',
        data: $("#PackageCheckoutForm").serialize(),
        success: function (data) {
            $("#PromoCodeTotal").html(data.discount.toFixed(2));
            $("#PackageTotal").html(data.finalCost.toFixed(2));
        },
        error: function (jqXHR, textStatus, errorThrown) {
            ToastService.error("An error occurred while trying to validate your promo code. Please try again.");
        }
    });
}

var validatePackageCheckout = function(){
    var returnValue = true;
    if($("#NewSubscriberPassword").val() !== $("#NewSubscriberReenterPassword").val()){
        ToastService.error("The passwords you've entered do not match.");
        returnValue = false;
    }

    $("#PackageCheckoutForm input").each(function(){
        var pattern = new RegExp($(this).data("val-regex-pattern"));
        if(pattern !== undefined && pattern !== ""){
            var result = pattern.test($(this).val())
            if(result === false){
                ToastService.error($(this).data("val-regex"));
                returnValue = false;
            }
        }
        
    });

    return returnValue;
}