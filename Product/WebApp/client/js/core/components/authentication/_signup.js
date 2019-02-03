$("#SignUpForm").submit(function (e) {
    e.preventDefault();
    var toastrOptions = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-top-full-width",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "3000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };
    if ($("#SignUpComponent #Password").val() !== $("#SignUpComponent #ReenterPassword").val()) {
        toastr.error("The passwords you have entered do not match.", 'Whoops...', toastrOptions);
    }
    else {
        $.ajax({
            type: "POST",
            url: '/Home/CampaignSignUp',
            data: $(this).serialize(),
            success: function (html) {
                if (html.statusCode === "200") {
                    window.location.href = html.description;
                }
                else {
                    toastr.error(html.description, 'Whoops...', toastrOptions);
                }
            }
        });
    }
    
    

}); 

