var resumeUploadInProgress = false;
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


$(document).ready(function () {
    $('.overlay').hide();

        

    $(".add-work-history").on("click", function () {
        //$('.work-history-log').append('<div class="form-row"><div class="form-group col-md-6" ><input type="text" class="form-control" placeholder="Job Title"></div><div class="form-group col-md-6"><input type="text" class="form-control" placeholder="Organization"></div></div>');
    });
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

    $(window).keydown(function (event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            return false;
        }
    });

    setCarouselHeight();

    $(window).resize(function () {
        setCarouselHeight();
    });

    $('#SignupFlowCarousel').bind('slide.bs.carousel', function (e) {
        document.body.scrollTop = document.documentElement.scrollTop = 0;
    });

    $('#UploadedResume').change(function () {
        var file = $(this)[0].files[0];
        if (file) {
            if (IsValidFileType(file.name)) {
                $('#UploadedResumeText').html("<strong>You've attached:</strong> " + file.name);
                $('.appear-on-resume-attach').show();
                $('#ResumeNextButton').addClass('disabled');
                $('#ResumeNextButton a').removeAttr("href");
                $('#ResumeUploadDisclaimer').css("display", "inline-block");
            }
            else {
                $('#UploadedResumeText').html("<span class='invalid-entry'>Sorry, unfortunately we do not accept resumes of this file type.</span> <br /> Please upload a file in one of the following file types:  .doc, .docx, .odt, .pdf, .rtf, .tex, .txt, .wks, .wps, or .wpd.");
                $('#ResumeUploadDisclaimer').css("display", "none");
                $('#SubmitResumeUpload').hide();
            }                       
            setCarouselHeight();
        }
        $('#UploadedResumeLabel').hide();
        $('#ChangeResumeLabel').show();
    });

    $('#ResumeUploadForm').on('submit', function (e) {
        e.preventDefault();
        // Set flag to indicate resume upload is in progress 
        resumeUploadInProgress = true;
        // Setup a time out to clean things up in case SignalR never calls back 
        ResumeUploadTimeout();
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
                        removeSkipFunctionalityOnResumeUpload();
                        toastr.success('Processing, please wait!', 'Success!', toastrOptions);
                        break;
                    default:
                        EnableResumeNextButton();
                        toastr.warning('We were not able to process this file. Please select another file and try again. Alternatively, you can skip this and move to the next step.', 'Warning!', toastrOptions);
                        break;
                }
            },
            error: function (jqXHR) {
                EnableResumeNextButton();
                toastr.error('Oops! Something unexpected happened, and we are looking into it.', 'Error!', toastrOptions);
            },
            complete: function (jqXHR, status) {
            }
        });
    });

    // Wire up SignalR to listen for resume upload completion
    CareerCircleSignalR.connect($("#hdnSubscriberGuid").val() )
        .then(result => {
             CareerCircleSignalR.listen("UploadResume", ResumeUploadComplete);
        });  
});



var EnableResumeNextButton = function () {
    $('#ResumeNextButton').removeClass('disabled');
    $('#ResumeNextButton a').attr("href", "#SignupFlowCarousel");
    $('.skip-resume').hide();
}

var ResumeUploadComplete = function (message) {
    // Make sure a resume upload in progress 
    if (resumeUploadInProgress == true) {
        toastr.success("All set, let's go!", 'Success!', toastrOptions);
        // Set flag to indicate resume upload is in complete 
        resumeUploadInProgress = false;
        EnableResumeNextButton();
        // Move to next page 
        $('.carousel').carousel({}).carousel('next');
    }
    else 
        toastr.success("Sorry, that took a little longer than we had hoped for...", 'Success!', toastrOptions);
}

// Set timeout in case 
var ResumeUploadTimeout = function () {
    setTimeout(function ()
    {
        // If the resume upload is still in progress after 10 seconds,
        // enable the next button
        if (resumeUploadInProgress == true) {
            resumeUploadInProgress = false;
            EnableResumeNextButton();
        }
    }, 15000);
}


var IsValidFileType = function (filename) {
    if (filename === null || filename === "" || !filename.includes('.')) {
        return false;
    }
    var fileExtensions = ["doc", "docx", "odt", "pdf", "rtf", "tex", "txt", "wks", "wps", "wpd"];
    var splitFileName = filename.split(".");
    if (fileExtensions.indexOf(splitFileName[splitFileName.length - 1]) > 0) {
        return true;
    }
    return false;
};

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

var findMaxCarouselItemHeight = function () {
    var maxHeight = 0;
    $(".carousel-item").each(function () {
        if ($(this).height() > maxHeight) {
            maxHeight = $(this).height();
        }
    });
    //add height from top of viewport
    maxHeight += 150;
    return maxHeight;
};

var setCarouselHeight = function () {
    $('.carousel-item').css("height", 'initial');
    $('.carousel-item').css("height", findMaxCarouselItemHeight());
};