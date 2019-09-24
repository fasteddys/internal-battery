$('.job-details-container #UploadedResume').change(function () {
    var file = $(this)[0].files[0];
    if (file) {
        if (IsValidFileType(file.name)) {
            $('#UploadedResumeText').html("<strong>You've attached:</strong> " + file.name);
            $('.appear-on-resume-attach').show();
        }
        else {
            $('#UploadedResumeText').html("<span class='invalid-entry'>Sorry, unfortunately we do not accept resumes of this file type.</span> <br /> Please upload a file in one of the following file types:  .doc, .docx, .odt, .pdf, .rtf, .tex, .txt, .wks, .wps, or .wpd.");
        }                       
    }
    $('#UploadedResumeLabel').hide();
    $('#ChangeResumeLabel').show();
});

var IsValidFileType = function (filename) {
    if (filename === null || filename === "" || !filename.includes('.')) {
        return false;
    }
    var fileExtensions = ["doc", "docx", "odt", "pdf", "rtf", "tex", "txt", "wks", "wps", "wpd"];
    var splitFileName = filename.split(".");
    if (fileExtensions.indexOf(splitFileName[splitFileName.length - 1]) >= 0) {
        return true;
    }
    return false;
};

$("#JobApplication button[type=submit]").on("click", function(e){
    e.preventDefault();
    

    if($("#FirstName").val() === "" || $("#FirstName").val() === undefined){
        ToastService.warning("Please enter your first name.");
        return;
    }

    if($("#LastName").val() === "" || $("#LastName").val() === undefined){
        ToastService.warning("Please enter your last name.");
        return;
    }

    if($("#HasResume").data("resume") === "False" && $("#UploadedResume")[0].files[0] === undefined){
        ToastService.warning("Please attach your resume.");
        return;
    }

    if($("#CoverLetter").val() === "" || $("#CoverLetter").val() === undefined){
        ToastService.warning("Please enter cover letter text.");
        return;
    }

    if($("#HasResume").data("resume") === "False" && $("#UploadedResume")[0].files[0] !== undefined){
        CareerCircleAPI.uploadResume($("#UploadedResume")[0].files[0], true)
            .catch((err) => {
                ToastService.error('Unable to submit application: error uploading your resume. Please try again.');
                return;
            });
    }

    $("#JobApplication").submit();
});