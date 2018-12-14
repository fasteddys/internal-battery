var workHistory = {};

$(document).ready(function () {
    $(".add-work-history").on("click", function () {
        //$('.work-history-log').append('<div class="form-row"><div class="form-group col-md-6" ><input type="text" class="form-control" placeholder="Job Title"></div><div class="form-group col-md-6"><input type="text" class="form-control" placeholder="Organization"></div></div>');
    });
    $('.save-new-work-history-item').on("click", function () {
        $(".work-history-log .row").append('<div class="col-12 col-sm-6 col-md-4"><div class="card"><div class= "card-body"><h5 class="card-title">Software Engineer</h5><h6 class="card-subtitle mb-2 text-muted">Allegis Group</h6><p class="card-text">Build an application in the .Net Core 2.1 framework.</p></div></div></div>');
    });

    $('#UploadedResume').change(function () {
        var file = $(this)[0].files[0];
        if (file) {
            $('#UploadedResumeText').html("<strong>You've attached:</strong> " + file.name);
            $('.appear-on-resume-attach').show();
            $('#SignupFlowNextButton').addClass('disabled');
            $('#SignupFlowNextButton a').removeAttr("href");
        }
        $('#UploadedResumeLabel').html('<i class="fas fa-exchange-alt"></i>&nbsp;&nbsp;Change');
    });

    $('#ResumeUploadForm').on('submit', function (e) {
        e.preventDefault();
        var formData = new FormData();
        formData.append('file', $('#UploadedResume')[0].files[0]); // myFile is the input type="file" control

        var _url = $(this).attr('action');

        $.ajax({
            url: _url,
            type: 'POST',
            data: formData,
            processData: false,  // tell jQuery not to process the data
            contentType: false,  // tell jQuery not to set contentType
            success: function (result) {
                var code = parseInt(result.statusCode);
                switch (code) {
                    case 200:
                        $('#SignupFlowNextButton').removeClass('disabled');
                        $('#SignupFlowNextButton a').attr("href", "#SignupFlowCarousel");
                        break;
                    case 400:
                        break;
                }
            },
            error: function (jqXHR) {
                alert("failure.");
            },
            complete: function (jqXHR, status) {
            }
        });
    });
});
