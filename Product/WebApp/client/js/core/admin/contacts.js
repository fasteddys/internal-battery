function importContacts() {
    $.ajax({
        type: 'PUT',
        url: contactsUrl + '/import/' + $('#partnerGuid').val() + '/' + encodeURIComponent($('#cacheKey').val()),
        success: function (results) {
            // toast message for success - display import results
        },
        error: function () {
            // toast message on error - clear form - etc
        }
    });
}

$(function () {

    var ul = $('#upload ul');

    $('#drop a').click(function () {
        // Simulate a click on the file input button
        // to show the file browser dialog
        $(this).parent().find('input').click();
    });

    // Initialize the jQuery File Upload plugin
    $('#upload').fileupload({

        // This element will accept file drag/drop uploading
        dropZone: $('#drop'),

        // This function is called when a file is added to the queue;
        // either via the browse button, or via drag/drop:
        add: function (e, data) {

            
            // Append the file name and file size
            tpl.find('p').text(data.files[0].name)
                .append('<i>' + formatFileSize(data.files[0].size) + '</i>');

            // Add the HTML to the UL element
            data.context = tpl.appendTo(ul);

            // Initialize the knob plugin
            tpl.find('input').knob();

            // Listen for clicks on the cancel icon
            tpl.find('span').click(function () {

                if (tpl.hasClass('working')) {
                    jqXHR.abort();
                }

                tpl.fadeOut(function () {
                    tpl.remove();
                });

            });

            // Automatically upload the file once it is added to the queue
            var jqXHR = data.submit();
        },

        progress: function (e, data) {

            // Calculate the completion percentage of the upload
            var progress = parseInt(data.loaded / data.total * 100, 10);

            // Update the hidden input field and trigger a change
            // so that the jQuery knob plugin knows to update the dial
            data.context.find('input').val(progress).change();

            if (progress === 100) {
                data.context.removeClass('working');
            }
        },

        done: function (e, data) {
            $("#drop").hide();
            $("#ContactsPreviewContainer").show();
            // load the preview data for the contacts 
            var contactsPreview = data.result.contactsPreview;
            var isHeaderProcessed = false;
            contactsPreview.forEach(function (row) {
                // create table header (if it does not yet exist)
                if (!isHeaderProcessed) {
                    Object.keys(row).forEach(function (key, idx) {
                        if (key === 'metadata') {
                            Object.keys(row[key]).forEach(function (metadataKey, i) {
                                $('#contactTablePreview thead tr').append('<th scope="col" style="background-color: yellow">' + metadataKey + '</th>');
                            });
                        } else {
                            $('#contactTablePreview thead tr').append('<th scope="col">' + key + '</th>');
                        }
                    });
                    isHeaderProcessed = true;
                }
                // create table rows
                var rowHtml = '<tr>';
                Object.keys(row).forEach(function (key, idx) {
                    if (key === 'metadata') {
                        Object.keys(row[key]).forEach(function (metadataKey, i) {
                            rowHtml += '<td>';
                            rowHtml += (row[key])[metadataKey];
                            rowHtml += '</td>';
                        });
                    } else {
                        rowHtml += '<td>';
                        rowHtml += row[key];
                        rowHtml += '</td>';
                    }
                });
                rowHtml += '</tr>';
                $('#contactTablePreview tbody').append(rowHtml);
            });

            // show summary of validation actions which will occur if the file is processed (e.g. removed duplicate rows by email)
            var importActions = data.result.importActions;
            importActions.forEach(function (importAction, idx) {
                var importActionHtml = '<li>';
                // todo: come up with a better way to format messages
                importActionHtml += importAction.count + ' records identified for ' + importAction.importBehavior + '(' + importAction.message + ')';
                importActionHtml += '</li>';
                $('#importValidationSummary').append(importActionHtml);
            });

            // set cache key for import operation
            $('#cacheKey').val(data.result.cacheKey);
        },

        fail: function (e, data) {
            // todo: add toast message?
            data.context.addClass('error');
        }
    });

    // Prevent the default action when a file is dropped on the window
    $(document).on('drop dragover', function (e) {
        e.preventDefault();
    });

    // Helper function that formats the file sizes
    function formatFileSize(bytes) {
        if (typeof bytes !== 'number') {
            return '';
        }

        if (bytes >= 1000000000) {
            return (bytes / 1000000000).toFixed(2) + ' GB';
        }

        if (bytes >= 1000000) {
            return (bytes / 1000000).toFixed(2) + ' MB';
        }

        return (bytes / 1000).toFixed(2) + ' KB';
    }

});