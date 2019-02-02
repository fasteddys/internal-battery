$("#ButtonComponent").on("click", function () {
    var form = $(this).data("form");
    $("#" + form).submit();
});