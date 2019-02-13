$("#ButtonComponent").on("click", function (e) {
    e.preventDefault();
    var form = $(this).data("form");
    $("#" + form).submit();
});