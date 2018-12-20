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

$(document).ajaxStart(function () { 
    timer = setTimeout(function () { $('.overlay').show(); }, 200);
}).ajaxStop(function () {
    clearTimeout(timer);
    $('.overlay').hide();
});

$(document).ready(function () {
    $('.overlay').hide();

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
            $('#ResumeNextButton').addClass('disabled');
            $('#ResumeNextButton a').removeAttr("href");
            $('#ResumeUploadDisclaimer').css("display", "inline-block");
        }
        $('#UploadedResumeLabel').hide();
        $('#ChangeResumeLabel').show();
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
                switch (result.statusCode) {
                    // update behavior based on revamped status codes
                    case 'Processing':
                        $('#ResumeNextButton').removeClass('disabled');
                        $('#ResumeNextButton a').attr("href", "#SignupFlowCarousel");
                        $('.skip-resume').hide();
                        removeSkipFunctionalityOnResumeUpload();
                        toastr.success('We are processing your resume now. The information we are able to extract will be displayed on your profile page.', 'Success!', toastrOptions);
                        break;
                    default:
                        toastr.warning('We were not able to process this file. Please select another file and try again. Alternatively, you can skip this and move to the next step.', 'Warning!', toastrOptions);
                        break;
                }
            },
            error: function (jqXHR) {
                toastr.error('Oops! Something unexpected happened, and we are looking into it.', 'Error!', toastrOptions);
            },
            complete: function (jqXHR, status) {
            }
        });
    });
});

var goToNextPhoneNumberInput = function(thisInputField, nextInputField){
    if ($('#' + thisInputField).val().length === 3) {
        $('#' + nextInputField).focus();
    }
};

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

var removeSkipFunctionalityOnResumeUpload = function () {
    $('#SkipResumeSlide').addClass('disabled');
    $('#SkipResumeSlide a').removeAttr("href");
    $('#SkipResumeSlide a').removeAttr("onclick");
};

var clearInputForResumeSlide = function () {
    $('#UploadedResume').val("");
    $('.appear-on-resume-attach').hide();
    $('#UploadedResumeLabel').show();
    $('#ChangeResumeLabel').hide();
    $('#ResumeNextButton').addClass('disabled');
    $('#ResumeNextButton a').removeAttr("href");
    $('#ResumeUploadDisclaimer').css("display", "none");
};