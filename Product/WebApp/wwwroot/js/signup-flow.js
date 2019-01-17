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

    

    $(".signup-flow-container input").keyup(function () {
        var parentSlide = $(this).closest(".carousel-item");
        var inputValue = $(this).val();
        var inputRegEx = new RegExp($(this).data("val-regex-pattern"));
        var regexTest = inputRegEx.test($(this).val());
        var isPhoneNumberSlide = $(this).attr("id") === "FormattedPhone";
        var containerName = $(this).data("container-name");

        

        if (inputFieldsAreValid(containerName)) {
            $(this).removeClass("invalid-regex");
            enableNextButton(parentSlide);
            $('.' + containerName + '-container').find(".input-disclaimer").hide();
        }
        else {
            disableNextButton(parentSlide);
            $('.' + containerName + '-container').find(".input-disclaimer").show();
        }
    });

    $(".signup-flow-container input").focusout(function () {
        var inputValue = $(this).val();
        var inputRegEx = new RegExp($(this).data("val-regex-pattern"));
        var regexTest = inputRegEx.test($(this).val());
        if ($(this).attr('id') === 'FormattedPhone') {
            regexTest = new RegExp($("#Phone").data("val-regex-pattern")).test(getTrimmedPhoneNumber($(this).val()));
        }
        if (!regexTest && inputValue) {
            $(this).addClass("invalid-regex");
        }
        else {
            $(this).removeClass("invalid-regex");
        }
    });



    $("#SelectedState").change(function () {

        var parentSlide = $(this).closest(".carousel-item");
        var nextButton = $(parentSlide).find(".flow-next-button");
        var nextButtonAnchor = $(parentSlide).find(".flow-next-button a");
        var isAddressSlide = $(this).parents(".address-container").length;
        if (isAddressSlide) {
            if (addressFieldsAreValid()) {
                $(nextButtonAnchor).attr("href", "#SignupFlowCarousel");
                $(nextButton).removeClass("disabled");
                $('.address-container').find(".input-disclaimer").hide();
            }
            else {
                $(nextButtonAnchor).removeAttr("href");
                $(nextButton).addClass("disabled");
                $('.address-container').find(".input-disclaimer").show();
            }
        }
    });

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

var disableNextButton = function (carouselSlide) {
    var parentSlide = $(carouselSlide).closest(".carousel-item");
    var nextButton = $(parentSlide).find(".flow-next-button");
    var nextButtonAnchor = $(parentSlide).find(".flow-next-button a");
    $(nextButtonAnchor).removeAttr("href");
    $(nextButton).addClass("disabled");
};

var enableNextButton = function (carouselSlide) {
    var parentSlide = $(carouselSlide).closest(".carousel-item");
    var nextButton = $(parentSlide).find(".flow-next-button");
    var nextButtonAnchor = $(parentSlide).find(".flow-next-button a");
    $(nextButtonAnchor).attr("href", "#SignupFlowCarousel");
    $(nextButton).removeClass("disabled");
};

var inputFieldsAreValid = function (containerName) {
    switch (containerName) {
        case "name":
            return nameFieldsAreValid();
        case "address":
            return addressFieldsAreValid();
        case "phone-number":
            return phoneNumberFieldsAreValid();
    }
};

var addressFieldsAreValid = function () {

    var allFieldsNotEmpty = $("#Address").val() && $("#City").val() && $("#SelectedState").val();
    var allFieldsEmpty = !$("#Address").val() && !$("#City").val() && !$("#SelectedState").val();

    return allFieldsEmpty || allFieldsNotEmpty;
};

var nameFieldsAreValid = function () {

    var firstNameElement = $("#SignupFlowCarousel #FirstNameInput");
    var lastNameElement = $("#SignupFlowCarousel #LastNameInput");


    // Allow user to enter in no address information
    if (!firstNameElement.val() && !lastNameElement.val())
        return true;

    var fNameValid = new RegExp(firstNameElement.data("val-regex-pattern")).test(firstNameElement.val());
    var lNameValid = new RegExp(lastNameElement.data("val-regex-pattern")).test(lastNameElement.val());
    if (firstNameElement.val() && fNameValid && lastNameElement.val() && lNameValid) {
        return true;
    }
    return false;
};

var phoneNumberFieldsAreValid = function () {
    var phoneNumber = $("#FormattedPhone").val();
    if (phoneNumber.length === 0)
        return true;
    var validLength = phoneNumber.length === 14;
    var inputRegEx = new RegExp($('#Phone').data("val-regex-pattern"));
    var regexTest = inputRegEx.test(getTrimmedPhoneNumber(phoneNumber));
    return validLength && regexTest;
};

var getTrimmedPhoneNumber = function (phoneNumber) {
    var strippedNumber = phoneNumber.replace("(", "").replace(")", "").replace(" ", "").replace("-", "");
    return strippedNumber;
};



var EnableResumeNextButton = function () {
    $('#ResumeNextButton').removeClass('disabled');
    $('#ResumeNextButton a').attr("href", "#SignupFlowCarousel");
    $('.skip-resume').hide();
};

var ResumeUploadComplete = function (message) {
    var json = JSON.parse(message);
    // Make sure a resume upload in progress 
    if (resumeUploadInProgress == true) {
        toastr.success("All set, let's go!", 'Success!', toastrOptions);
        // Set flag to indicate resume upload is in complete 
        resumeUploadInProgress = false;
        EnableResumeNextButton();
        //Populate later slides with information extracted from resume parse
        PopulateOnboardingSlides(json);
        // Move to next page 
        $('.carousel').carousel({}).carousel('next');
    }
    else
        toastr.success("Sorry, that took a little longer than we had hoped for...", 'Success!', toastrOptions);
};

var PopulateOnboardingSlides = function (response) {
    var carousel = $("#SignupFlowCarousel");
    carousel.find("#FirstNameInput").val(SetProperCase(response.firstName));
    carousel.find("#LastNameInput").val(SetProperCase(response.lastName));
    if (response.PhoneNumber) {
        carousel.find("#FormattedPhone").val("("
            + response.PhoneNumber.substring(0, 3) + ") "
            + response.PhoneNumber.substring(3, 6) + "-"
            + response.PhoneNumber.substring(6, 10));
    }

    carousel.find("#Address").val(SetProperCase(response.address));
    carousel.find("#City").val(SetProperCase(response.city));

    if (response.state && response.state.stateGuid) {
        carousel.find("#SelectedState").val(response.state.stateGuid);
    }

    

    var subscriberSkills = response.skills;
    var selectize = carousel.find("#SelectedSkills")[0].selectize;
    selectize.clear();
    selectize.load(function (callback) {
        callback(subscriberSkills);
    });
    var selectedSkills = [];
    $.each(subscriberSkills, function (i, obj) {
        selectedSkills.push(obj.skillGuid);
    });
    selectize.setValue(selectedSkills);
};

var SetProperCase = function (value) {
    if (value)
        return value.toProperCase();
    return value;
};

// Set timeout in case 
var ResumeUploadTimeout = function () {
    setTimeout(function () {
        // If the resume upload is still in progress after 10 seconds,
        // enable the next button
        if (resumeUploadInProgress == true) {
            resumeUploadInProgress = false;
            EnableResumeNextButton();
        }
    }, 15000);
};


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

String.prototype.toProperCase = function () {
    return this.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase(); });
};