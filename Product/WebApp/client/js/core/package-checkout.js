$( document ).ready(function() {
    $("#PackageAgreeToTermsAndConditionsCheckbox").prop("checked", false);
    

    $('#PackageAgreeToTermsAndConditionsCheckbox').change(function() {
        $("#PackageAgreeToTermsAndConditions").val(this.checked);
        if(this.checked) {
            $(".authenticated-section input").not(".promo-code-entered").prop( "disabled", false );
            $(".authenticated-section select").prop( "disabled", false );
            $("#PackageCheckout").prop( "disabled", false );
        }
        else{
            $(".authenticated-section input").not(".promo-code-entered").prop( "disabled", true );
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
    $(".bt-drop-reset-instruction").hide();
    $(".overlay").show();
    $.ajax({
        type: 'POST',
        url: $("#PackageCheckoutForm").attr('action'),
        data: $("#PackageCheckoutForm").serialize(),
        success: function (data) {
            $(".overlay").hide();
            if(data.statusCode == 200){
                document.location.href = "/career-services/" + pageSlug + "/confirmation/" + data.description;
            }
            else{
                ToastService.error(data.description);
            }

            if(data.statusCode == 410){
                $(".bt-drop-reset-instruction").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $(".overlay").hide();
            ToastService.error("Something went wrong with your checkout.");
        }
    });
}

var validatePromoCode = function(){
    if($("#PromoCodeEntered").val() === undefined || $("#PromoCodeEntered").val() === ""){
        ToastService.warning("Please enter a promo code and try again.");
        return;
    }

    $(".overlay").show();
    $.ajax({
        type: 'POST',
        url: '/services/promo-code/validate',
        data: $("#PackageCheckoutForm").serialize(),
        success: function (data) {
            $(".overlay").hide();
            $("#ValidationMessageSuccess span").html("");
            $("#ValidationMessageError span").html("");
            if(data.isValid){
                $("#PromoCodeTotal").html(data.discount.toFixed(2));
                $("#PackageTotal").html(data.finalCost.toFixed(2));
                $("#ValidationMessageSuccess span").html(data.validationMessage);
            }
            else{
                $("#PromoCodeTotal").html(0.00);
                $("#PackageTotal").html($("#InitialPackagePrice").html());
                $("#ValidationMessageError span").html(data.validationMessage);
            }
            if(data.finalCost === 0 && data.isValid){
                $("#BraintreePaymentContainer").hide();
            }
            else{
                $("#BraintreePaymentContainer").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $(".overlay").hide();
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