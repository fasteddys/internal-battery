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
                zoom: 4.8,
                disableDefaultUI: true,
                styles: [
                    {
                      "elementType": "geometry",
                      "stylers": [
                        {
                          "color": "#1d2c4d"
                        }
                      ]
                    },
                    {
                      "elementType": "labels.text.fill",
                      "stylers": [
                        {
                          "color": "#8ec3b9"
                        }
                      ]
                    },
                    {
                      "elementType": "labels.text.stroke",
                      "stylers": [
                        {
                          "color": "#1a3646"
                        }
                      ]
                    },
                    {
                      "featureType": "administrative.country",
                      "elementType": "geometry.stroke",
                      "stylers": [
                        {
                          "color": "#4b6878"
                        }
                      ]
                    },
                    {
                      "featureType": "administrative.land_parcel",
                      "stylers": [
                        {
                          "visibility": "off"
                        }
                      ]
                    },
                    {
                      "featureType": "administrative.land_parcel",
                      "elementType": "labels.text.fill",
                      "stylers": [
                        {
                          "color": "#64779e"
                        }
                      ]
                    },
                    {
                      "featureType": "administrative.locality",
                      "stylers": [
                        {
                          "visibility": "off"
                        }
                      ]
                    },
                    {
                      "featureType": "administrative.neighborhood",
                      "stylers": [
                        {
                          "visibility": "off"
                        }
                      ]
                    },
                    {
                      "featureType": "administrative.province",
                      "elementType": "geometry.stroke",
                      "stylers": [
                        {
                          "color": "#4b6878"
                        }
                      ]
                    },
                    {
                      "featureType": "landscape.man_made",
                      "elementType": "geometry.stroke",
                      "stylers": [
                        {
                          "color": "#334e87"
                        }
                      ]
                    },
                    {
                      "featureType": "landscape.natural",
                      "elementType": "geometry",
                      "stylers": [
                        {
                          "color": "#023e58"
                        }
                      ]
                    },
                    {
                      "featureType": "poi",
                      "elementType": "geometry",
                      "stylers": [
                        {
                          "color": "#283d6a"
                        }
                      ]
                    },
                    {
                      "featureType": "poi",
                      "elementType": "labels.text",
                      "stylers": [
                        {
                          "visibility": "off"
                        }
                      ]
                    },
                    {
                      "featureType": "poi",
                      "elementType": "labels.text.fill",
                      "stylers": [
                        {
                          "color": "#6f9ba5"
                        }
                      ]
                    },
                    {
                      "featureType": "poi",
                      "elementType": "labels.text.stroke",
                      "stylers": [
                        {
                          "color": "#1d2c4d"
                        }
                      ]
                    },
                    {
                      "featureType": "poi.business",
                      "stylers": [
                        {
                          "visibility": "off"
                        }
                      ]
                    },
                    {
                      "featureType": "poi.park",
                      "elementType": "geometry.fill",
                      "stylers": [
                        {
                          "color": "#023e58"
                        }
                      ]
                    },
                    {
                      "featureType": "poi.park",
                      "elementType": "labels.text.fill",
                      "stylers": [
                        {
                          "color": "#3C7680"
                        }
                      ]
                    },
                    {
                      "featureType": "road",
                      "stylers": [
                        {
                          "visibility": "off"
                        }
                      ]
                    },
                    {
                      "featureType": "road",
                      "elementType": "geometry",
                      "stylers": [
                        {
                          "color": "#304a7d"
                        }
                      ]
                    },
                    {
                      "featureType": "road",
                      "elementType": "labels",
                      "stylers": [
                        {
                          "visibility": "off"
                        }
                      ]
                    },
                    {
                      "featureType": "road",
                      "elementType": "labels.icon",
                      "stylers": [
                        {
                          "visibility": "off"
                        }
                      ]
                    },
                    {
                      "featureType": "road",
                      "elementType": "labels.text.fill",
                      "stylers": [
                        {
                          "color": "#98a5be"
                        }
                      ]
                    },
                    {
                      "featureType": "road",
                      "elementType": "labels.text.stroke",
                      "stylers": [
                        {
                          "color": "#1d2c4d"
                        }
                      ]
                    },
                    {
                      "featureType": "road.highway",
                      "elementType": "geometry",
                      "stylers": [
                        {
                          "color": "#2c6675"
                        }
                      ]
                    },
                    {
                      "featureType": "road.highway",
                      "elementType": "geometry.stroke",
                      "stylers": [
                        {
                          "color": "#255763"
                        }
                      ]
                    },
                    {
                      "featureType": "road.highway",
                      "elementType": "labels.text.fill",
                      "stylers": [
                        {
                          "color": "#b0d5ce"
                        }
                      ]
                    },
                    {
                      "featureType": "road.highway",
                      "elementType": "labels.text.stroke",
                      "stylers": [
                        {
                          "color": "#023e58"
                        }
                      ]
                    },
                    {
                      "featureType": "transit",
                      "stylers": [
                        {
                          "visibility": "off"
                        }
                      ]
                    },
                    {
                      "featureType": "transit",
                      "elementType": "labels.text.fill",
                      "stylers": [
                        {
                          "color": "#98a5be"
                        }
                      ]
                    },
                    {
                      "featureType": "transit",
                      "elementType": "labels.text.stroke",
                      "stylers": [
                        {
                          "color": "#1d2c4d"
                        }
                      ]
                    },
                    {
                      "featureType": "transit.line",
                      "elementType": "geometry.fill",
                      "stylers": [
                        {
                          "color": "#283d6a"
                        }
                      ]
                    },
                    {
                      "featureType": "transit.station",
                      "elementType": "geometry",
                      "stylers": [
                        {
                          "color": "#3a4762"
                        }
                      ]
                    },
                    {
                      "featureType": "water",
                      "elementType": "geometry.fill",
                      "stylers": [
                        {
                          "color": "#03264A"
                        }
                      ]
                    }
                  ]
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