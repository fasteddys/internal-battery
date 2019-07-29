class USMap {
    constructor(model) {
        this.model = model;
    }

    initialize() {
        for (var i = 0; i < this.model.length; i++) {
            var prefix = this.model[i].statePrefix;
            var companyPosting = this.model[i].companyPosting;
            var total = this.model[i].totalCount;
            var mappoint = '#' + prefix + '.map-point';     
            $(mappoint + ' a').html(total);
            var options = {
                html: true,
                animation: true,
                placement: "top",
                content: this.buildContent(name, prefix, companyPosting, total)
            };
            $(mappoint).popover(options);
            $(mappoint).show();
        }
    }

    buildContent(name, prefix, cp, total) {
        var url = "browse-jobs-location/us/" + prefix + "/1";
        var string = '<div class="map-popover">' +
            '<div class="map-popover-total"><a rel="nofollow" href="' + url + '"> Total Jobs: <b>' + total + '</b></a></div>' +
            '<div class="map-popover-company">' + this.buildCompanyDiv(cp, name) + '</div>' +
            '</div>';
        return string;
    }

    buildCompanyDiv(cp, name) {
        var str = "";
        for (var c in cp) {
            var url = "jobs?keywords=" + cp[c].companyName + "&location=" + name;
            str += '<div>' +
                '<a rel="nofollow" href="' + url + '">' +
                cp[c].companyName + ': <b>' + cp[c].jobCount.toString() +
                '</b></a>' +
                '</div>';
        }
        return str;
    }
}