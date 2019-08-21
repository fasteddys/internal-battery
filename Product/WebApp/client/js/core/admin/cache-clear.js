function clearCmsCache(){
    var page = 
    $.ajax({
        type: 'POST',
        url: '/admin/ClearCmsCacheAsync',
        contentType: "application/json",
        data: JSON.stringify({
            Slug: $("#CachedPage").val()
        }),
        success: function (data) {
            ToastService.success("CMS page " + $("#CachedPage").val() + " has been cleared!");
        },
        error: function (jqXHR, textStatus, errorThrown) {
            ToastService.error("Cache clear failure.");
        }
    });
}

$('#CachedPage').keyup(function(e){
    if(e.keyCode == 13)
    {
        clearCmsCache();
    }
});
