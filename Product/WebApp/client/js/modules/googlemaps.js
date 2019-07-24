class GMap {
    markers = [];
    infowindows = [];
    constructor(model, baseurl) {
        this.model = model;
        this.baseurl = baseurl;
        this.uscords = mapdata.unitedstates.cords;
        this.map = new google.maps.Map(document.getElementById('map'),
            {
                center: this.uscords,
                zoom: 3.8,
                disableDefaultUI: true,
                styles: mapstyle
            });
        this.initializeMap();
    }

    initializeMap() {
        for (var i = 0; i < this.model.length; i++) {
            var key = this.model[i].stateId;
            var st = mapdata.states[key];
            var ct = this.model[i].totalCount;
            var content = this.buildContent(st.name, st.prefix, this.model[i].companyPosting, ct);
            this.addMarker(st.cords.lat, st.cords.lng, st.name, ct, content);
        }
    }

    addMarker(lat, lng, title, ct, content) {
        var infowindow = new google.maps.InfoWindow(
            {
                content: content
            });

        var marker = new google.maps.Marker(
            {
                position:
                {
                    lat: lat,
                    lng: lng
                },
                animation: google.maps.Animation.DROP,
                label: ct.toString(),
                title: title
            });

        this.markers.push(marker);
        marker.setMap(this.map);
        marker.addListener('click', function () {
            infowindow.open(this.map, marker);
        });
    }

    buildContent(stname, stprefix, cp, total) {
        var sturl = this.baseurl + "browse-jobs-location/us/" + stprefix + "/1";
        var string = '<div class="gmap-container-infowindow">' +
            '<div class="gmap-container-infowindow-title">' + stname + '</div>' +
            '<div class="gmap-container-infowindow-total"><a rel="nofollow" href="' + sturl + '"> Total : ' + total + '</a></div>' +
            '<div class="gmap-container-infowindow-company">' + this.buildCompanyDiv(cp, stname) + '</div>' +
            '</div>';
        return string;
    }

    buildCompanyDiv(cp, stname) {
        var str = "";
        for (var c in cp) {
            var url = this.baseurl + "jobs?keywords=" + cp[c].companyName + "&location=" + stname;
            str += '<div>' +
                '<a rel="nofollow" href="' + url + '">' +
                cp[c].companyName + ' : ' + cp[c].jobCount.toString() +
                '</a>' +
                '</div>';
        }
        return str;
    }
}