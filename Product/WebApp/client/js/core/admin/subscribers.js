function deleteSubscriber(subscriberGuid, cloudIdentifier) {
    bootbox.confirm({
        message: "Are you sure you want to delete this subscriber?",
        buttons: {
            confirm: {
                label: 'Yes',
                className: 'btn-success'
            },
            cancel: {
                label: 'No',
                classname: 'btn-danger'
            }
        },
        callback: function (result) {
            if (result) {
                $.ajax({
                    url: url + "/" + subscriberGuid + "/" + cloudIdentifier,
                    method: "DELETE",
                    success: function (data, textStatus, jqXHR) {
                        if (textStatus === "success") {
                            ToastService.success("The subscriber may still appear in search results for a few minutes after the deletion.", "Subscriber deleted");
                            doSearch(false);
                        } else {
                            ToastService.warning("The subscriber was not deleted successfully", "Something went wrong");
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        ToastService.error(textStatus, errorThrown);
                    }
                });
            }
        }
    });
}