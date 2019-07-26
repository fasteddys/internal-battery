class USMap {
    constructor(model, baseurl) {
        this.model = model;
        this.baseurl = baseurl;
    }

    initialize() {
        for (var i = 0; i < this.model.length; i++) {
            var prefix = this.model[i].statePrefix;
            var companyPosting = this.model[i].companyPosting;
            var total = this.model[i].totalCount;
            var mappoint = '#' + prefix + '.map-point';
            var name = $(mappoint).attr('title');
            $(mappoint + ' a').html(total);
            var options = {
                placement: 'right',
                title: name,
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
        var url = this.baseurl + "browse-jobs-location/us/" + prefix + "/1";
        var string = '<div class="gmap-container-infowindow">' +
            '<div class="gmap-container-infowindow-total"><a rel="nofollow" href="' + url + '"> Total : ' + total + '</a></div>' +
            '<div class="gmap-container-infowindow-company">' + this.buildCompanyDiv(cp, name) + '</div>' +
            '</div>';
        return string;
    }

    buildCompanyDiv(cp, name) {
        var str = "";
        for (var c in cp) {
            var url = this.baseurl + "jobs?keywords=" + cp[c].companyName + "&location=" + name;
            str += '<div>' +
                '<a rel="nofollow" href="' + url + '">' +
                cp[c].companyName + ' : ' + cp[c].jobCount.toString() +
                '</a>' +
                '</div>';
        }
        return str;
    }
}