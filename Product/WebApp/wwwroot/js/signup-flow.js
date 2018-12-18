var workHistory = {};
class WorkHistoryItem {
    constructor(_title, _org, _description) {
        this.title = _title;
        this.organization = _org;
        this.description = _description;
    }
}

var education = {};
class Education {
    constructor(_institution, _major) {
        this.institution = _institution;
        this.major = _major;
    }
}

$(document).ready(function () {
    $(".add-work-history").on("click", function () {
        //$('.work-history-log').append('<div class="form-row"><div class="form-group col-md-6" ><input type="text" class="form-control" placeholder="Job Title"></div><div class="form-group col-md-6"><input type="text" class="form-control" placeholder="Organization"></div></div>');
    });
    /*
    $('#SignupFlowCarousel').bind('slide.bs.carousel', function (e) {
        if ($('.carousel-item-second').hasClass('active')) {
            $('.previous-container').removeClass("hidden");
            $('.previous-container').removeClass("slideInUp");
            $('.previous-container').addClass("slideOutDown");
        }
        else {
            $('.previous-container').removeClass("slideOutDown");
            $('.previous-container').addClass("slideInUp");
        }
    });*/
    $('.save-new-work-history-item').on("click", function () {
        var jobTitle = $('#JobTitleInput').val();
        var organization = $('#OrganizationInput').val();
        var description = $('#JobDescriptionInput').val();
        if (jobTitle && organization && description) {
            $('#WorkHistoryModal').modal("hide");
            workHistory["WorkHistoryItem" + objectSize(workHistory)] = new WorkHistoryItem(jobTitle, organization, description);
            $(".work-history-log .row").append('<div class="col-12 col-sm-6 col-md-4"><div class="card"><div class= "card-body"><h5 class="card-title">' + jobTitle + '</h5><h6 class="card-subtitle mb-2 text-muted">' + organization + '</h6><p class="card-text">' + description + '</p></div></div></div>');
            clearWorkHistoryModal();
            $('#WorkHistoryInput').val(JSON.stringify(workHistory));
        }
        else {
            $('.work-history-modal-validation').show();
        }
        
    });

    $('.save-new-education-item').on("click", function () {
        var institution = $('#InstitutionInput').val();
        var major = $('#MajorInput').val();
        if (institution && major) {
            $('#EducationModal').modal("hide");
            education["EducationItem" + objectSize(education)] = new Education(institution, major);
            $(".education-history-log .row").append('<div class="col-12 col-sm-6 col-md-4"><div class="card"><div class= "card-body"><h5 class="card-title">' + institution + '</h5><h6 class="card-subtitle mb-2 text-muted">' + major + '</h6></div></div></div>');
            clearEducationModal();
            $('#EducationInput').val(JSON.stringify(education));
        }
        else {
            $('.education-modal-validation').show();
        }

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
        formData.append('Resume', $('#UploadedResume')[0].files[0]); // myFile is the input type="file" control

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

var clearWorkHistoryModal = function () {
    $('#WorkHistoryModal').find("input").val("");
    $('#WorkHistoryModal').find("textarea").val("");

};

var clearEducationModal = function () {
    $('#EducationModal').find("input").val("");
    $('#EducationModal').find("textarea").val("");

};

var closeModals = function () {
    $('.modal').modal("hide");
};

var objectSize = function (obj) {
    var size = 0, key;
    for (key in obj) {
        if (obj.hasOwnProperty(key)) size++;
    }
    return size;
};