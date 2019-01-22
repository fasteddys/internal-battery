$(document).ready(function () {

    $('.work-history-tenure').each(function () {
        this.innerHTML = FormattedDateRange($(this).data('startdate'), $(this).data('enddate'));
    });

    $('.work-history-compensation').each(function () {
        this.innerHTML = FormattedCompensation($(this).data('compensationtype'), $(this).data('compensation'));
    });

    var showChar = 250;  // How many characters are shown by default
    var ellipsestext = "...";
    var moretext = "Show more >";
    var lesstext = "Show less";

    $('.more').each(function () {
        var content = $(this).html();

        if (content.length > showChar) {

            var c = content.substr(0, showChar);
            var h = content.substr(showChar, content.length - showChar);

            var html = c + '<span class="moreellipses">' + ellipsestext + '&nbsp;</span><span class="morecontent"><span>' + h + '</span>&nbsp;&nbsp;<a href="" class="morelink">' + moretext + '</a></span>';

            $(this).html(html);
        }
    });

    $(".morelink").click(function () {
        if ($(this).hasClass("less")) {
            $(this).removeClass("less");
            $(this).html(moretext);
        } else {
            $(this).addClass("less");
            $(this).html(lesstext);
        }
        $(this).parent().prev().toggle();
        $(this).prev().toggle();
        return false;
    });

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
                $(this).parents('div[class^="profile-edit-modal-edit-option"]').eq(0).addClass("regex-failure");
                $("#" + modalParentId + "SaveButton").prop('disabled', true);
            }
            else {
                $(this).parents('div[class^="profile-edit-modal-edit-option"]').eq(0).removeClass("regex-failure");
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
    $("#txtWorkHistoryJobDescription").val("");
    $("#ddlWorkHistoryCompany")[0].selectize.clear();
    $("#txtWorkHistoryStartDate").val("");
    $("#txtWorkHistoryEndDate").val("");
    $("#chkWorkHistoryIsCurrent").prop("checked", false);
    $("#txtWorkHistoryJobTitle").val("");
    $("#txtWorkHistoryJobDescription").val("");
    $("#txtWorkHistoryCompensation").val("");
    $("#ddlWorkHistoryCompensationType").val($("#ddlWorkHistoryCompensationType option:first").val());
}
function CreateWorkHistoryDto(includeGuid) {
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
            toastr.warning(JSON.stringify(data), 'Oops, Something went wrong.');
        },
        success: function (res) {
            var html = CreateWorkHistoryDiv(res);
            $("#ProfileWorkHistory").append(html);
            if ($("#ProfileWorkHistory").children().length <= 0)
                $("#ProfileWorkHistoryNotSpecified").hide();
        }
    });

    $('#WorkHistoryModal').modal('hide');
    return false;
}

function DeleteWorkHistory(WorkHistoryGuid) {
    bootbox.confirm({
        message: "Are you sure you want to delete this work history entry?",
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
                    url: '/Home/DeleteWorkHistory/' + WorkHistoryGuid,
                    type: 'POST',
                    contentType: "application/json",
                    error: function (data) {
                        toastr.warning(JSON.stringify(data), 'Oops, Something went wrong.');
                    },
                    success: function (res) {
                        $("#ProfileWorkHistory_" + res.subscriberWorkHistoryGuid).remove();
                        if ($("#ProfileWorkHistory").children().length <= 0)
                            $("#ProfileWorkHistoryNotSpecified").show();
                    }
                });
            }
        }
    });
}

function FormattedDateRange(startDate, endDate) {
    var formattedDateRange = '';
    var effectiveStartDate;
    if (!moment(startDate, "MM/DD/YYYY").isValid()) {
        return 'No date range specified';
    }
    else {
        formattedDateRange = moment(startDate).format("MMMM YYYY") + " - ";
        effectiveStartDate = moment(startDate);
    }
    var effectiveEndDate;

    if (!moment(endDate, "MM/DD/YYYY").isValid()) {
        effectiveEndDate = moment();
        formattedDateRange += "Present";
    } else {
        effectiveEndDate = moment(endDate);
        formattedDateRange += moment(endDate).format("MMMM YYYY");
    }
    var years = 0;
    var months = 0;
    if (effectiveEndDate > effectiveStartDate) {
        years = effectiveEndDate.diff(effectiveStartDate, 'years');
        effectiveStartDate.add(years, 'years');
        months = effectiveEndDate.diff(effectiveStartDate, 'months');
        effectiveStartDate.add(months, 'months');
        formattedDateRange += " (" + years + " years " + months + " months)";
    }
    else {
        return "Invalid date range specified";
    }

    return formattedDateRange;
}

function FormattedCompensation(compensationType, compensation) {
    var formattedCompensation = '';
    if (compensation === '' || compensation === '0') {
        return "No compensation specified";
    } else {
        var formatter = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD',
            minimumFractionDigits: 2
        });
        formattedCompensation = formatter.format(compensation);
        if (!(compensationType === '')) {
            formattedCompensation += ' (' + compensationType + ')';
        }
        return formattedCompensation;
    }
}

function CreateWorkHistoryDiv(WorkHistoryInfo) {

    var divHtml = "<div class=\"row profile-work-history\" id=\"ProfileWorkHistory_@wh.SubscriberWorkHistoryGuid\">";
    divHtml += "<div id=\"ProfileWorkHistory_Title_@wh.SubscriberWorkHistoryGuid\" class=\"col-11 work-history-title\" data-title=\"@wh.Title\">";
    divHtml += "@wh.Title";
    divHtml += "<i class=\"fa fa-pencil-alt\" aria-hidden=\"true\" onclick=\"EditWorkHistory('@wh.SubscriberWorkHistoryGuid')\"></i>";
    divHtml += "<i class=\"fa fa-trash\" aria-hidden=\"true\" onclick=\"DeleteWorkHistory('@wh.SubscriberWorkHistoryGuid')\"></i>";
    divHtml += "</div>";
    divHtml += "<div id=\"ProfileWorkHistory_Company_@wh.SubscriberWorkHistoryGuid\" class=\"col-11 work-history-company\" data-company=\"@wh.Company\">";
    divHtml += "@wh.Company";
    divHtml += "</div>";
    divHtml += "<div id=\"ProfileWorkHistory_Tenure_@wh.SubscriberWorkHistoryGuid\" class=\"col-11 work-history-tenure\" data-startdate=\"@wh.StartDate\" data-enddate=\"@wh.EndDate\" data-iscurrent=\"@wh.IsCurrent\">";
    divHtml += FormattedDateRange(WorkHistoryInfo.startDate, WorkHistoryInfo.startDate);
    divHtml += "</div>";
    divHtml += "<div id=\"ProfileWorkHistory_Compensation_@wh.SubscriberWorkHistoryGuid\" class=\"col-11 work-history-compensation\" data-compensation=\"@wh.Compensation\" data-compensationtype=\"@wh.CompensationType\">";
    divHtml += FormattedCompensation(WorkHistoryInfo.compensationType, WorkHistoryInfo.compensation);
    divHtml += "</div>";
    divHtml += "<div id=\"ProfileWorkHistory_Description_@wh.SubscriberWorkHistoryGuid\" class=\"col-11 work-history-description more\" data-description=\"@wh.JobDecription\">";
    divHtml += "@wh.JobDecription";
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
    var startDate = moment($("#ProfileWorkHistory_Tenure_" + WorkHistoryGuid).data("startdate")).format('MM/DD/YYYY');
    if (startDate === "Invalid date")
        startDate = null;
    var endDate = moment($("#ProfileWorkHistory_Tenure_" + WorkHistoryGuid).data("enddate")).format('MM/DD/YYYY');
    if (endDate === "Invalid date")
        endDate = null;
    var isCurrent = $("#ProfileWorkHistory_Tenure_" + WorkHistoryGuid).data("iscurrent");
    var jobTitle = $("#ProfileWorkHistory_Title_" + WorkHistoryGuid).data("title");
    var jobDescription = $("#ProfileWorkHistory_Description_" + WorkHistoryGuid).data("description");
    var compensationType = $("#ProfileWorkHistory_Compensation_" + WorkHistoryGuid).data("compensationtype");
    var compensation = $("#ProfileWorkHistory_Compensation_" + WorkHistoryGuid).data("compensation");

    // Populate edit form with values 
    $('#ddlWorkHistoryCompany')[0].selectize.createItem(company, false)
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


