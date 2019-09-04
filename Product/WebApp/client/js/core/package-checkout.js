$( document ).ready(function() {
    
    $("#PackageCheckoutAgreeTerms").val(this.checked);

    $('#PackageCheckoutAgreeTerms').change(function() {
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


    $("#PackageCheckoutForm").find("input[type='submit']").on("click", function(){
        alert("hello world!");
    });
    
});

var calculatePackagePrice = function(){
    var InitialPackagePrice = $("#InitialPackagePrice").html();
    var PromoCodeTotal = $("#PromoCodeTotal").html();
    return InitialPackagePrice - PromoCodeTotal;
}