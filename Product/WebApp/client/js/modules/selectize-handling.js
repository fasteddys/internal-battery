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
            $('.overlay').show(); 
            $.ajax({
                url: '/Home/GetSkills',
                type: 'GET',
                dataType: 'json',
                data: {
                    userQuery: encodeURIComponent(query)
                },
                error: function () {
                    callback();
                    $('.overlay').hide(); 
                },
                success: function (res) {
                    callback(res);
                    $('.overlay').hide(); 
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
});

