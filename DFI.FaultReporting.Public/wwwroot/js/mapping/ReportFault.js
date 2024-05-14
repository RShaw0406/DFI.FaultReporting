// CODE SUMMARY:
// This file controls the fault selection map.A user can search for locations on the map using the autocomplete search,
// when they click on a suggestion returned by the autocomplete the map will zoom to that location. A user can then click an area
// on the map to add a pin, if the pin is not placed on a road it will be snapped to the closest road to the user click. The lat/long
// of the pin will then be stored along with information on the road itself.

// Azure Maps Subscription Key - Needed to access the service
const azureMapsSubscriptionKey = "tVwRA8vhqB9AvHiYgZa1muR90phLPrp6qzmJFvjqa0Q";

// Global variables used throughout code
var map, marker, searchURL

// Define map boundaries - this will restrict how far a user can pan on the map control and also the search results returned in the autocomplete
const mapBounds = [-8.3, 53.9, -5.3, 55.4]

// Array of different road layers provided by the Azure Maps Service - All are included here to ensure we have full coverage
var roadLayers = [
    'road',
    'Connecting road',
    'Connecting road tunnel',
    'International road',
    'International road tunnel',
    'Local road',
    'Local road tunnel',
    'Major local road',
    'Major local road tunnel',
    'Major road',
    'Major road tunnel',
    'Minor local road',
    'Minor local road tunnel',
    'Motorway',
    'Motorway tunnel',
    'Secondary road',
    'Secondary road tunnel',
    'Toll connecting road',
    'Toll connecting road tunnel',
    'Toll international road',
    'Toll international road tunnel',
    'Toll local road',
    'Toll local road tunnel',
    'Toll major local road',
    'Toll major local road tunnel',
    'Toll major road',
    'Toll major road tunnel',
    'Toll minor local road',
    'Toll minor local road tunnel',
    'Toll motorway',
    'Toll motorway tunnel',
    'Toll secondary road',
    'Toll secondary road tunnel',
    'Parking road'
];

// FUNCTION SUMMARY:
// This method is used for setting up the map control and adding event listeners to the map - uses the Azure Maps Web SDK
function initMap() {
    // Create new map control
    map = new atlas.Map('map', {
        // Center map on Northern Ireland
        center: [-6.8, 54.65],
        // Set zoom to 3 to show as much of Northern Ireland as possible
        zoom: 3,
        // Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland
        maxBounds: mapBounds,
        // Set lanuage to GB for better localisation
        language: 'en-GB',
        // Set auth
        authOptions: {
            authType: 'subscriptionKey',
            subscriptionKey: azureMapsSubscriptionKey
        },
        style: 'road'
    });

    // Add zoom control to the map
    map.controls.add(new atlas.control.ZoomControl(), {
        position: 'bottom-right'
    });

    // Add full screen control to the map
    map.controls.add(new atlas.control.FullscreenControl(), {
        position: 'top-right',
        style: 'auto'
    });

    map.controls.add(new atlas.control.StyleControl(), {
        position: 'top-left',
        mapStyles: 'all'
    });

    // Add click event to the map
    map.events.add('click', function (e) {
        // Get the lat/long position of the click event
        var position = [e.position[0], e.position[1]];

        // Snap the a marker to the closest road using the lat/long position of the click
        snapMarkerToRoad(position);
    });

    // Use MapControlCredential to share authentication between a map control and the service module
    var pipeline = atlas.service.MapsURL.newPipeline(new atlas.service.MapControlCredential(map));

    // Create an instance of the SearchURL client
    searchURL = new atlas.service.SearchURL(pipeline);
}

// Initialise the map
initMap();

// FUNCTION SUMMARY:
// This method is used for setting up the autocomplete search box
function initSearch() {
    // Find search box on page
    var searchBox = document.getElementById('searchBox');

    // Add event for when user begins to input text
    searchBox.addEventListener('input', search);
}

// Initialise the search
initSearch();


function search() {
    var query = document.getElementById('searchBox').value;

    if (query == "") {
        map.setCamera({
            // Center map on Northern Ireland
            center: [-6.8, 54.65],
            // Set zoom to 3 to show as much of Northern Ireland as possible
            zoom: 3,
            // Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland
            maxBounds: mapBounds,
        });
    }
    else {
        searchURL.searchFuzzy(atlas.service.Aborter.timeout(10000), query, {
            lat: 54.65,
            lon: -6.8,
            btmRight: '53.9,-8.3',
            topLeft: '55.4,-5.3',
            radius: 100000,
            view: 'Auto',
            language: 'en-GB'
        }).then(results => {
            console.log(results);
            if (results.results.length > 0) {
                autocomplete(document.getElementById("searchBox"), results);
            }
            else {
                map.setCamera({
                    // Center map on Northern Ireland
                    center: [-6.8, 54.65],
                    // Set zoom to 3 to show as much of Northern Ireland as possible
                    zoom: 3,
                    // Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland
                    maxBounds: mapBounds,
                });
            }
        });
    }
}

function autocomplete(inp, results) {
    var geoData = results.geojson.getFeatures();

    console.log(geoData);
    /*the autocomplete function takes two arguments,
    the text field element and an array of possible autocompleted values:*/
    var currentFocus;
    var a, b, i, val = inp.value;
    /*close any already open lists of autocompleted values*/
    closeAllLists();
    if (!val) { return false; }
    currentFocus = -1;
    /*create a DIV element that will contain the items (values):*/
    a = document.createElement("DIV");
    a.setAttribute("id", inp.id + "autocomplete-list");
    a.setAttribute("class", "autocomplete-items");
    /*append the DIV element as a child of the autocomplete container:*/
    inp.parentNode.appendChild(a);
    /*for each item in the array...*/
    for (i = 0; i < geoData.features.length; i++) {
        if (geoData.features[i].properties.address.countrySubdivisionName == "Northern Ireland") {
            /*create a DIV element for each matching element:*/
            b = document.createElement("DIV");
            /*make the matching letters bold:*/
            b.innerHTML = "<strong>" + geoData.features[i].properties.address.freeformAddress.substr(0, val.length) + "</strong>";
            b.innerHTML += geoData.features[i].properties.address.freeformAddress.substr(val.length);
            /*insert a input field that will hold the current array item's value:*/
            b.innerHTML += "<input type='hidden' value='" + geoData.features[i].properties.address.freeformAddress + "'>";

            //var geoData = results[i].geojson.getFeatures();

            console.log(geoData.features[i].bbox);

            b.innerHTML += "<input type='hidden' value='" + geoData.features[i].bbox + "'>";
            /*execute a function when someone clicks on the item value (DIV element):*/
            b.addEventListener("click", function (e) {
                /*insert the value for the autocomplete text field:*/
                inp.value = this.getElementsByTagName("input")[0].value;
                /*close the list of autocompleted values,
                (or any other open lists of autocompleted values:*/
                //Set the camera to the bounds of the results.

                var boundingBox = this.getElementsByTagName("input")[1].value

                console.log(boundingBox);

                const boundingBoxArray = boundingBox.split(",");

                map.setCamera({
                    bounds: boundingBoxArray,
                    // Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland
                    maxBounds: mapBounds,
                });
                closeAllLists();
            });
            a.appendChild(b);
        }
    }
    /*execute a function presses a key on the keyboard:*/
    inp.addEventListener("keydown", function (e) {
        var x = document.getElementById(inp.id + "autocomplete-list");
        if (x) x = x.getElementsByTagName("div");
        if (e.keyCode == 40) {
            /*If the arrow DOWN key is pressed,
            increase the currentFocus variable:*/
            currentFocus++;
            /*and and make the current item more visible:*/
            addActive(x);
        } else if (e.keyCode == 38) { //up
            /*If the arrow UP key is pressed,
            decrease the currentFocus variable:*/
            currentFocus--;
            /*and and make the current item more visible:*/
            addActive(x);
        } else if (e.keyCode == 13) {
            /*If the ENTER key is pressed, prevent the form from being submitted,*/
            e.preventDefault();
            if (currentFocus > -1) {
                /*and simulate a click on the "active" item:*/
                if (x) x[currentFocus].click();
            }
        }
    });
    function addActive(x) {
        /*a function to classify an item as "active":*/
        if (!x) return false;
        /*start by removing the "active" class on all items:*/
        removeActive(x);
        if (currentFocus >= x.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = (x.length - 1);
        /*add class "autocomplete-active":*/
        x[currentFocus].classList.add("autocomplete-active");
    }
    function removeActive(x) {
        /*a function to remove the "active" class from all autocomplete items:*/
        for (var i = 0; i < x.length; i++) {
            x[i].classList.remove("autocomplete-active");
        }
    }
    function closeAllLists(elmnt) {
        /*close all autocomplete lists in the document,
        except the one passed as an argument:*/
        var x = document.getElementsByClassName("autocomplete-items");
        for (var i = 0; i < x.length; i++) {
            if (elmnt != x[i] && elmnt != inp) {
                x[i].parentNode.removeChild(x[i]);
            }
        }
    }

    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}

function snapMarkerToRoad(position) {

    //The max distance a coordinate needs to be from a road in meters if it is to snap to it.
    const maxSnappingDistance = 10;

    var lines;

    var originalPosition = new atlas.data.Feature(new atlas.data.Point(position));

    var point = originalPosition

    //Only retrieve the line data from the map if a point feature needs to be snapped. This is a simple optimization.
    if (!lines) {
        //Retrieve all rendered line data from the map.
        lines = map.layers.getRenderedShapes(null, null, ['any', ['==', ['geometry-type'], 'LineString'], ['==', ['geometry-type'], 'MultiLineString']]);
    }

    var minDistance = Number.MAX_VALUE;
    var closestCoord = point.geometry.coordinates;

    //Snap to the closest road.
    for (var i = 0, len = lines.length; i < len; i++) {
        //Ensure the layer has a sourceLayer (indicates its from a vector tile source) and that the source layer is one of the maps base map layers.
        if (lines[i].sourceLayer && roadLayers.indexOf(lines[i].sourceLayer) !== -1) {
            //Get the closest point on the source layer to the original source coordinate of the point feature.
            var cp = atlas.math.getClosestPointOnGeometry(point.geometry.coordinates, lines[i]);

            //Ensure that the closest point is closer than the previously calculated snapped coordinates for the point.
            if (cp && cp.properties.distance <= maxSnappingDistance && cp.properties.distance < minDistance) {

                //console.log("Point Details:")
                //console.log(cp);

                console.log("Marker Location Details:")
                console.log(cp);

                //Capture the distance and closest coordinate.
                minDistance = cp.properties.distance;
                closestCoord = cp.geometry.coordinates;
                document.getElementById('LabelLat').innerHTML = closestCoord[1];
                document.getElementById('LabelLong').innerHTML = closestCoord[0];
                document.getElementById('LabelPlace').innerHTML = closestCoord;

                reverseAddressSearch(closestCoord);   
            }
        }
    }
}

// METHOD SUMMARY - This method is used for placing the marker for the fault on the map control
function placeMarker(position) {

    map.markers.clear();

    marker = new atlas.HtmlMarker({
        position: position
    });

    map.markers.add(marker);
}

function reverseAddressSearch(position) {

    searchURL.searchAddressReverse(atlas.service.Aborter.timeout(3000), position, {
        view: 'Auto',
        radius: 6
    }).then(results => {

        console.log(results);

        console.log("Snapped Location Results:");

        var result = results.geojson.getFeatures();

        console.log(result);

        if (result.features[0].properties.address.countrySubdivisionName == "Northern Ireland") {

            placeMarker(position);

            if (result.features.length > 0 && result.features[0].properties && result.features[0].properties.address) {
                var road1 = (result.features[0].properties.address.streetName);
                var road2 = (result.features[0].properties.address.localName);
                var road3 = (result.features[0].properties.address.countrySecondarySubdivision);
                document.getElementById('LabelRoad').innerHTML = road1 + ", " + road2 + ", " + road3;
                console.log(result.features[0].properties.address.freeformAddress);
            } else {
                console.log("No address for that location!");
            }
        }
        else {
            alert('Please select a road within Northern Ireland')
        }
    });
}