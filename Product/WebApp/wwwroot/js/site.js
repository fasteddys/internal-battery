
$(document).ready(function () {

    $('#SelectedSkills').selectize({
        valueField: 'skillGuid',
        labelField: 'skillName',
        searchField: 'skillName',
        persist: false,
        loadThrottle: 600,
        create: false,
        options: subscriberSkills, // initialized on Profile.cshtml
        allowEmptyOption: false,
        delimiter: ',',
        load: function (query, callback) {
            if (!query.length) return callback();
            $.ajax({
                url: '/Home/GetSkills',
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
        },
        onInitialize: function () {
            var selectize = this;
            var selectedSkills = [];
            $.each(subscriberSkills, function (i, obj) {
                selectedSkills.push(obj.skillGuid);
            });
            selectize.setValue(selectedSkills);
        }
    });



    $('#phone-number')

        .keydown(function (e) {
            var key = e.which || e.charCode || e.keyCode || 0;
            $phone = $(this);

            // Don't let them remove the starting '('
            if ($phone.val().length === 1 && (key === 8 || key === 46)) {
                $phone.val('(');
                return false;
            }
            // Reset if they highlight and type over first char.
            else if ($phone.val().charAt(0) !== '(') {
                $phone.val('(' + String.fromCharCode(e.keyCode) + '');
            }

            // Auto-format- do not expose the mask as the user begins to type
            if (key !== 8 && key !== 9) {
                if ($phone.val().length === 4) {
                    $phone.val($phone.val() + ')');
                }
                if ($phone.val().length === 5) {
                    $phone.val($phone.val() + ' ');
                }
                if ($phone.val().length === 9) {
                    $phone.val($phone.val() + '-');
                }
            }

            // Allow numeric (and tab, backspace, delete) keys only
            return (key == 8 ||
                key == 9 ||
                key == 46 ||
                (key >= 48 && key <= 57) ||
                (key >= 96 && key <= 105));
        })

        .bind('focus click', function () {
            $phone = $(this);

            if ($phone.val().length === 0) {
                $phone.val('(');
            }
            else {
                var val = $phone.val();
                $phone.val('').val(val); // Ensure cursor remains at the end
            }
        })

        .blur(function () {
            $phone = $(this);

            if ($phone.val() === '(') {
                $phone.val('');
            }
        });

    $('#SelectedCountry').change(function () {
        var selectedCountry = $("#SelectedCountry").val();
        var stateSelect = $("#SelectedState");

        stateSelect.empty();
        if (selectedCountry != null && selectedCountry != '') {
            $.getJSON(url, { countryGuid: selectedCountry }, function (states) {
                if (states != null && !jQuery.isEmptyObject(states)) {
                    stateSelect.append($('<option/>', {
                        value: "---",
                        text: "Select State"
                    }));
                    $.each(states.value, function (index, state) {
                        stateSelect.append($('<option/>', {
                            value: state.stateGuid,
                            text: state.name
                        }));
                    });
                    var savedStateGuid = $("#SavedStateGuid").val();
                    if (savedStateGuid === undefined || savedStateGuid === "00000000-0000-0000-0000-000000000000" || savedStateGuid === "") {
                        $("#SelectedState").val("---");
                    }
                    else {
                        $("#SelectedState").val(savedStateGuid);
                    }
                };
            });
        }
    });

    // select the country that is currently associated with the subscriber, otherwise default to USA
    var savedCountryGuid = $("#SelectedCountry").val();
    if (savedCountryGuid === undefined || savedCountryGuid === "00000000-0000-0000-0000-000000000000" || savedCountryGuid === "") {
        $("#SelectedCountry option:eq(1)").attr('selected', 'selected');
    } else {
        $("#SelectedCountry").val(savedCountryGuid);
    }

    $("input[name='SelectedCourseVariant']").change(function () {
        var selectedCourseVariant = $("input[name='SelectedCourseVariant']:checked");
        var selectedCourseVariantPrice = $(selectedCourseVariant).parent().next().children(".price").html();
        $("#InitialCoursePrice").html(selectedCourseVariantPrice);

        if ($("#PromoCodeTotal").html().startsWith("-$")) {
            var initial = parseFloat($("#InitialCoursePrice").html().replace(",", "").split("$")[1]);
            var discount = Math.min(parseFloat($("#PromoCodeTotal").html().replace(",", "").split("$")[1]), initial);
            $("#PromoCodeTotal").html("-$" + discount.toFixed(2));
            $("#CourseTotal").html("$" + ((initial - discount)).toFixed(2));
        } else {
            $("#CourseTotal").html(selectedCourseVariantPrice);
        }

        // display any child elements with class "CourseVariantStartDate", hide all sibling child elements with "CourseVariantStartDate"
        $(selectedCourseVariant).parent().parent().children(".CourseVariantStartDate").show();
        $(selectedCourseVariant).parent().parent().siblings().children(".CourseVariantStartDate").hide();

        if ($("#CourseTotal").html() === "$0.00")
            $('#BraintreePaymentContainer').hide();
        else
            $('#BraintreePaymentContainer').show();
    });

    $("#SameAsAboveCheckbox").change(function () {
        if (this.checked) {
            $('.billing-info-container').hide();
            $('.billing-info-container input, .billing-info-container select').removeAttr('required');
        }
        else {
            $('.billing-info-container').show();
            $('.billing-info-container input, .billing-info-container select').prop('required', true);
        }
    });

    // Uncheck checkbox on page load.
    $('#TermsOfServiceCheckbox').prop('checked', false);
    $("#TermsOfServiceCheckbox").change(function () {
        if (this.checked) {
            $('#EnrollmentSubmitButton').prop('disabled', false);
        }
        else {
            $('#EnrollmentSubmitButton').prop('disabled', true);
        }
    });


    $('.edit-profile-info-button').on('click', function () {
        $('.personal-info-display').slideToggle();
        $('#PersonalInfoFormContainer').slideToggle();
        $('.edit-profile-info-button').toggleClass('expanded');

        $('.edit-profile-info-button').each(function () {
            if ($(this).hasClass("expanded") && !$(this).hasClass('profile-mobile-view')) {
                $(this).html("Cancel");
            } else {
                if (!$(this).hasClass('profile-mobile-cancel')) {

                    $(this).html("Edit");
                }
            }
        });

    });

    $('#PersonalInfoForm').submit(function (e) {
        $.ajax({
            type: "POST",
            url: '/home/UpdateProfileInformation',
            data: $(this).serialize(),
            success: function (html) {
                if (html.statusCode === "200") {
                    location.reload();
                }
                else {
                    $('#ValidationSummary ul').html('');
                    var errorMessages = html.description.split("|");
                    for (i = 0; i < errorMessages.length; i++) {
                        if (i != errorMessages.length - 1) {
                            var li = document.createElement("li");
                            li.innerHTML = errorMessages[i];
                            $('#ValidationSummary ul').append(li);
                        }

                    }
                }
            }
        });
        e.preventDefault();

    });
    $('.play-button').on('click', function () {
        $(this).hide();
        $('.enrollment-success-video-thumbnail').hide();
        $('#EnrollmentSuccessVideo').prop('controls', true);
        document.getElementById("EnrollmentSuccessVideo").play();
    });

    //$('.courses-in-progress-list-desktop li').find(".course-listing").animate({ width: '0' });

    $('.courses-in-progress-list-desktop li').each(function () {
        var progressBar = $(this).find(".progress-bar");
        var newWidth = $(progressBar).attr('aria-valuenow') + "%";
        $(progressBar).animate({ width: newWidth });
    });



    $('#PromoCodeApplyButton').on('click', function () {
        var _promoCode = $('#PromoCodeInput').val();
        var _courseVariantGuid = $("input[name='SelectedCourseVariant']:checked").val();
        var _subscriberGuid = $('#SubscriberGuid').val();

        if (typeof _courseVariantGuid == 'undefined') {
            $('#ValidationMessageError span').html('A course section must be selected before applying a promo code.');
            $('#ValidationMessageSuccess').hide();
            $('#ValidationMessageError').show();
        } else if (_promoCode !== undefined && $.trim(_promoCode) !== '' && typeof _courseVariantGuid != 'undefined') {
            var form = $('#CourseCheckoutForm');
            var token = $('input[name="__RequestVerificationToken"]', form).val();
            var postUrl = "/Course/PromoCodeValidation/" + _promoCode + "/" + _courseVariantGuid + "/" + _subscriberGuid;

            $.ajax({
                url: postUrl,
                type: 'POST',
                contentType: 'application/x-www-form-urlencoded',
                headers: { 'RequestVerificationToken': token },
                success: function (result) {
                    if (result.isValid) {
                        $('#ValidationMessageSuccess span').html(result.validationMessage);
                        $('#ValidationMessageSuccess').show();
                        $('#ValidationMessageError').hide();
                        $('#PromoCodeTotal').html("-$" + result.discount.toFixed(2));
                        $('#CourseTotal').html("$" + result.finalCost.toFixed(2));
                        $('#PromoCodeRedemptionGuid').val(result.promoCodeRedemptionGuid);
                        $('#PromoCodeApplyButton').prop('disabled', true);
                        $('#PromoCodeApplyButton').css('color', 'white');
                        if (result.finalCost === 0)
                            $('#BraintreePaymentContainer').hide();
                    } else {
                        $('#ValidationMessageError span').html(result.validationMessage);
                        $('#ValidationMessageSuccess').hide();
                        $('#ValidationMessageError').show();
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $('#ValidationMessageError span').html('Woops! Something went wrong - please try another promo code.');
                    $('#ValidationMessageSuccess').hide();
                    $('#ValidationMessageError').show();
                }
            });
        }
        else {
            $('#ValidationMessageError span').html('No promotional code was supplied; please enter a value and try again.');
            $('#ValidationMessageSuccess').hide();
            $('#ValidationMessageError').show();
        }
    });
});


