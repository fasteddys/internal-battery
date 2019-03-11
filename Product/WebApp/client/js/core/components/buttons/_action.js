$(".button-container").on("click", function (e) {
    if ($(this).data("action-type") === "SUBMIT") {
        e.preventDefault();
        var form = $(this).data("form");
        $("#" + form).submit();
    }
    
});