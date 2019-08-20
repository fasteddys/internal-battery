function clearCmsCache(){
    $.ajax({
        type: 'POST',
        url: 'admin/ClearCmsCacheAsync',
        success: function (data) {
            ToastService.success("Cache cleared!");
        },
        error: function (jqXHR, textStatus, errorThrown) {
            ToastService.error("Cache clear failure.");
        }
    });
}
