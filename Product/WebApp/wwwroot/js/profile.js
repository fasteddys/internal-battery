
 

$(document).ready(function () {
    $(".modal .profile-edit-modal-edit-option input").keyup(function () {
 
        var modalParent = $(this).parents().eq(5);
        var modalParentId = modalParent.attr('id');
        var inputId = $(this).attr('id');
        if ($("#" + inputId).val() === "") {
            $(this).parent().parent().removeClass("regex-failure");
            var invalidClassExists = false;
            $("#" + modalParentId + " .profile-edit-modal-edit-option").each(function () {
                if ($(this).hasClass("regex-failure")) {
                    invalidClassExists = true;
                }
            });
            if (!invalidClassExists)
                $("#" + modalParentId + "SaveButton").prop('disabled', false);
        }
        else {
            var inputRegEx = new RegExp($(this).data("val-regex-pattern"));

            var regexTest = inputRegEx.test($(this).val());
            if (!regexTest) {
                $(this).parent().parent().addClass("regex-failure");
                $("#" + modalParentId + "SaveButton").prop('disabled', true);
            }
            else {
                $(this).parent().parent().removeClass("regex-failure");
                invalidClassExists = false;
                $("#" + modalParentId + " .profile-edit-modal-edit-option").each(function () {
                    if ($(this).hasClass("regex-failure")) {
                        invalidClassExists = true;
                    }
                });
                if (!invalidClassExists)
                    $("#" + modalParentId + "SaveButton").prop('disabled', false);
            }
        }
    });


   $('#ddlWorkHistoryCompany').selectize({
        valueField: 'companyName',
        labelField: 'companyName',
        searchField: 'companyName',
        persist: false,
        loadThrottle: 600,
        create: true,
        maxItems: 1,
        allowEmptyOption: false,
        delimiter: ',',
        load: function (query, callback) {
            if (!query.length) return callback();
            $.ajax({
                url: '/Home/GetCompanies',
                type: 'GET',
                dataType: 'json',
                data: {
                    userQuery: query
                },
                error: function () {
                    callback();
                },
                success: function (res) {
                    callback(res);
                }
            });
        }
    });

    $('#txtWorkHistoryStartDate').datepicker({
        autoclose: true
    });

    $('#txtWorkHistoryEndDate').datepicker({
        autoclose: true
    });

});

function InitWorkHistoryAddMode() {
    // Important! clear the the cached history guid which is 
    $("#hdnWorkHistoryGuid").val("");  
}
function CreateWorkHistoryDto(includeGuid)
{
    var isChecked = 0;
    if ($("#chkWorkHistoryIsCurrent").is(':checked'))
        isChecked = 1;

    var compensation = parseFloat($("#txtWorkHistoryCompensation").val());
    if (isNaN(compensation))
        compensation = 0;

    rval = {        
        StartDate: $("#txtWorkHistoryStartDate").val(),
        EndDate: $("#txtWorkHistoryEndDate").val(),
        IsCurrent: isChecked,
        Title: $("#txtWorkHistoryJobTitle").val(),
        JobDecription: $("#txtWorkHistoryJobDescription").val(),
        Compensation: compensation,
        CompensationType: $('#ddlWorkHistoryCompensationType').find(":selected").text(),
        Company: $('#ddlWorkHistoryCompany')[0].selectize.getValue() 
    }

    if (includeGuid)
        rval.SubscriberWorkHistoryGuid = $("#hdnWorkHistoryGuid").val();

    return rval;
}


function SaveWorkHistory() {
    
    // Check for add or edit 
    if ($("#hdnWorkHistoryGuid").val().trim() == "")
        AddWorkHistory();
    else
        UpdateWorkHistory();

    return false;
}


function UpdateWorkHistory() {  
    var obj = CreateWorkHistoryDto(true);
    var objJson = JSON.stringify(obj);

    $.ajax({
        url: '/Home/UpdateWorkHistory',
        type: 'POST',
        contentType: "application/json",
        data: objJson,
        error: function (data) {
            toastr.warning(JSON.stringify(data), 'Oops, Something went wrong.');
        },
        success: function (res) {
            var html = CreateWorkHistoryDiv(res);
            $("#ProfileWorkHistory_" + res.subscriberWorkHistoryGuid).replaceWith(html);
        }
    });

    $('#WorkHistoryModal').modal('hide');
    return false;
}

function AddWorkHistory() {
    var obj = CreateWorkHistoryDto(false);
    var objJson = JSON.stringify(obj);
  
    $.ajax({
        url: '/Home/AddWorkHistory',
        type: 'POST',
        contentType: "application/json",
        data: objJson,
        error: function (data) {
            toastr.warning(JSON.stringify(data) , 'Oops, Something went wrong.');
        },
        success: function (res) {
            var html = CreateWorkHistoryDiv(res);
            if ($("#ProfileWorkHistory").children().length > 0)
                $("#ProfileWorkHistory").children().last().append(html);
            else
                $("#ProfileWorkHistory").append(html);
        }
    });

    $('#WorkHistoryModal').modal('hide');
    return false;
}

function DeleteWorkHistory(WorkHistoryGuid) {
    $.ajax({
        url: '/Home/DeleteWorkHistory/' + WorkHistoryGuid,
        type: 'POST',
        contentType: "application/json",
        error: function (data) {
            toastr.warning(JSON.stringify(data), 'Oops, Something went wrong.');
        },
        success: function (res) {
            $("#ProfileWorkHistory_" + res.subscriberWorkHistoryGuid).remove();
        }
    });
}

// TODO find a way to not have to repeat this logic in C# and javascrip
function FormattedCompanyTenure(WorkHistoryInfo)
{
    var rVal = "";

    if (WorkHistoryInfo.startDate == "" && WorkHistoryInfo.endDate == "")
        return "Dates unknown";

    if (WorkHistoryInfo.startDate != "")
        rVal = WorkHistoryInfo.startDate
    else
        rVal += " ? ";

    rVal += " - ";

    if (WorkHistoryInfo.endDate != "")
        rVal += WorkHistoryInfo.endDate;
    else
        rVal += " ? ";

    if (WorkHistoryInfo.isCurrent > 0)
        rVal += " (current)";

    return rVal;  
}

function CreateWorkHistoryDiv(WorkHistoryInfo) {
    
    var divHtml =
       "<div class=\"row profile-work-history\" id=\"ProfileWorkHistory_@wh.SubscriberWorkHistoryGuid\">";
    divHtml += "<div id=\"ProfileWorkHistory_Company_@wh.SubscriberWorkHistoryGuid\" class=\"col-12 col-sm-9 option-value no-padding\" data-company=\"@wh.Company\">";
    divHtml += "@wh.Company <i class=\"fa fa-pencil-alt\" aria-hidden=\"true\" onclick=\"EditWorkHistory('@wh.SubscriberWorkHistoryGuid')\"></i>     <i class=\"fa fa-bone\" aria-hidden=\"true\" onclick=\"DeleteWorkHistory('@wh.SubscriberWorkHistoryGuid')\"> </i>";
    divHtml += "</div>";
    divHtml += "<div id=\"ProfileWorkHistory_Tenure_@wh.SubscriberWorkHistoryGuid\" class=\"col-12 col-sm-9 option-value no-padding\"   data-startdate=\"@wh.StartDate\" data-enddate=\"@wh.EndDate\" data-iscurrent=\"@wh.IsCurrent\"  >";
    divHtml += "@Model.FormattedCompanyTenure(@wh.StartDate, @wh.EndDate, @wh.IsCurrent)";
    divHtml += "</div>";
    divHtml += "<div id=\"ProfileWorkHistory_JobInfo_@wh.SubscriberWorkHistoryGuid\"  class=\"col-12 col-sm-9 option-value no-padding\">";
    divHtml += "<span id=\"ProfileWorkHistory_JobTitle_@wh.SubscriberWorkHistoryGuid\">   @wh.Title </span>  : <span id=\"ProfileWorkHistory_JobDescription_@wh.SubscriberWorkHistoryGuid\"> @wh.JobDecription </span>";
    divHtml += "</div>";
    divHtml += "<div class=\"col-12 col-sm-9 option-value no-padding\">";
    divHtml += " <span id=\"ProfileWorkHistory_CompensationType_@wh.SubscriberWorkHistoryGuid\"> @wh.CompensationType </span> compensation <span id=\"ProfileWorkHistory_Compensation_@wh.SubscriberWorkHistoryGuid\"> @wh.Compensation </span>";
    divHtml += "</div>";
    divHtml += "</div>";  
    // Replace razor items with values from new work history 
    var regex = /@wh.Company/gi;
    divHtml = divHtml.replace(regex, WorkHistoryInfo.company);

    regex = /@wh.SubscriberWorkHistoryGuid/gi;
    divHtml = divHtml.replace(regex, WorkHistoryInfo.subscriberWorkHistoryGuid);

    regex = /@wh.Title/gi;
    divHtml = divHtml.replace(regex, WorkHistoryInfo.title);

    regex = /@wh.CompensationType/gi;
    divHtml = divHtml.replace(regex, WorkHistoryInfo.compensationType);

    regex = /@wh.JobDecription/gi;
    divHtml = divHtml.replace(regex, WorkHistoryInfo.jobDecription);

    regex = /@wh.Compensation/gi;
    divHtml = divHtml.replace(regex, WorkHistoryInfo.compensation);

    // This regex is not working, just use the regular replace and research when time allows 
    regex = /@Model.FormattedCompanyTenure(@wh.StartDate, @wh.EndDate, @wh.IsCurrent)/gi;
    divHtml = divHtml.replace("@Model.FormattedCompanyTenure(@wh.StartDate, @wh.EndDate, @wh.IsCurrent)", FormattedCompanyTenure(WorkHistoryInfo));

    regex = /@wh.StartDate/gi;
    divHtml = divHtml.replace(regex, WorkHistoryInfo.startDate);

    regex = /@wh.EndDate/gi;
    divHtml = divHtml.replace(regex, WorkHistoryInfo.endDate);

    regex = /@wh.IsCurrent/gi;
    divHtml = divHtml.replace(regex, WorkHistoryInfo.isCurrent);
   
    return divHtml;
}




function EditWorkHistory(WorkHistoryGuid) {    
    // Retrieve values for history to edit from screen
    var company = $("#ProfileWorkHistory_Company_" + WorkHistoryGuid).data("company");
    var startDate =  $("#ProfileWorkHistory_Tenure_" + WorkHistoryGuid).data("startdate");
    var endDate = $("#ProfileWorkHistory_Tenure_" + WorkHistoryGuid).data("enddate");
    var isCurrent =  $("#ProfileWorkHistory_Tenure_" + WorkHistoryGuid).data("iscurrent");
    var jobTitle = $("#ProfileWorkHistory_JobTitle_" + WorkHistoryGuid).text().trim();
    var jobDescription = $("#ProfileWorkHistory_JobDescription_" + WorkHistoryGuid).text().trim();
    var compensationType = $("#ProfileWorkHistory_CompensationType_" + WorkHistoryGuid).text().trim();
    var compensation = $("#ProfileWorkHistory_Compensation_" + WorkHistoryGuid).text().trim();

    // Populate edit form with values 
    $('#ddlWorkHistoryCompany')[0].selectize.createItem(company,false) 
    $("#txtWorkHistoryStartDate").val(startDate);
    $("#txtWorkHistoryEndDate").val(endDate);
    if (isCurrent == "1")
        $('#chkWorkHistoryIsCurrent').prop('checked', true)
    else 
        $('#chkWorkHistoryIsCurrent').prop('checked', false)

    $("#txtWorkHistoryJobTitle").val(jobTitle);
    $("#txtWorkHistoryJobDescription").val(jobDescription);
    $("#txtWorkHistoryCompensation").val(compensation);    
    $("#ddlWorkHistoryCompensationType").val(compensationType);
    $('#WorkHistoryModal').modal('show');
    // Set Guid on edit form 
    $("#hdnWorkHistoryGuid").val(WorkHistoryGuid);      
}

function SelectDate(ctl) {   
    $('#' + ctl).datepicker('show');
}


