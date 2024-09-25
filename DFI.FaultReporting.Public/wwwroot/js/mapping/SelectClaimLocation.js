//CODE SUMMARY:
//This file controls the select claim location map. A user can search for locations on the map using the autocomplete search,
//when they click on a suggestion returned by the autocomplete the map will zoom to that location. A user can choose to select a location of the claim incident by clicking on the map.

//// Azure Maps Subscription Key - Needed to access the service
//const azureMapsSubscriptionKey = "tVwRA8vhqB9AvHiYgZa1muR90phLPrp6qzmJFvjqa0Q";

// Global variables used throughout code
var map, marker, searchURL

//// Define map boundaries - this will restrict how far a user can pan on the map control and also the search results returned in the autocomplete
//const mapBounds = [-8.3, 53.9, -5.3, 55.4]

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
function initClaimLocationMap() {
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

    // Add full screen control to the map
    map.controls.add(new atlas.control.FullscreenControl(), {
        position: 'bottom-right',
        style: 'auto'
    });

    // Add zoom control to the map
    map.controls.add(new atlas.control.ZoomControl(), {
        position: 'bottom-right'
    });

    map.controls.add(new atlas.control.StyleControl(), {
        position: 'bottom-right',
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

    CheckForSessionClaimLocationMarker()
}


// FUNCTION SUMMARY:
// This function is used to snap a marker to the closest road on the map.
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

                //Capture the distance and closest coordinate.
                minDistance = cp.properties.distance;
                closestCoord = cp.geometry.coordinates;

                // Do a reverse address search on the snapped location.
                reverseAddressSearch(closestCoord);
            }
        }
    }
}

//FUNCTION SUMMARY:
//This function is used to place a marker on the map at the given position.
function placeMarker(position) {

    //Clear any existing markers on the map, only 1 can be placed at any time.
    map.markers.clear();

    //Create a new marker at the given position.
    marker = new atlas.HtmlMarker({
        htmlContent: "<div><div class='marker'></div><div class='pulse'></div></div>",
        position: position,
        pixelOffset: [5, -18]
    });

    //Add the marker to the map.
    map.markers.add(marker);
}

// FUNCTION SUMMARY:
// This function is used to perform a reverse address search on a given position.
function reverseAddressSearch(position) {

    //Perform a reverse address search on the given position.
    searchURL.searchAddressReverse(atlas.service.Aborter.timeout(3000), position, {
        view: 'Auto',
        radius: 7
    }).then(results => {

        //Get the results of the reverse address search.
        var result = results.geojson.getFeatures();

        //Check if the result is in Northern Ireland.
        if (result.features[0].properties.address.countrySubdivisionName == "Northern Ireland") {

            document.getElementById('lat').value = position[1];
            document.getElementById('long').value = position[0];

            document.getElementById('selectionError').style.display = "none";

            //Place a marker on the map at the given position.
            placeMarker(position);
        }
        //If the result is not in Northern Ireland, display an error message to the user.
        else {
            document.getElementById('selectionError').style.display = "block";

            //Hide the error message after 3 seconds.
            setTimeout(() => {
                document.getElementById('selectionError').style.display = "none";
            }, 3000);
        }
    });
}

// FUNCTION SUMMARY:
// This function is used to check if there is a claim location marker in session storage and if so, place it on the map.
function CheckForSessionClaimLocationMarker() {

    // Get the claim from the session.
    if (claim != null)
    {
        // Check if the claim has a location.
        if (claim.incidentLocationLatitude != null && claim.incidentLocationLongitude)
        {
            //Create the position dictionary by getting the values of the lat/long hidden fields on the page.
            const position = [];
            position[0] = claim.incidentLocationLongitude;
            position[1] = claim.incidentLocationLatitude;

            //Create a marker to display the location of the fault.
            marker = new atlas.HtmlMarker({
                htmlContent: "<div><div class='marker'></div><div class='pulse'></div></div>",
                position: position,
                pixelOffset: [5, -18]
            });

            //Add the marker to the map.
            map.markers.add(marker);

            //Set the camera to the location of the fault.
            map.setCamera({
                // Center map on marker
                center: [Number(position[0]), Number(position[1])],
                // Set zoom to 18 to get close to marker
                zoom: 18,
                // Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland
                maxBounds: mapBounds,
            });
        }
    }

    // Check if the lat and long hidden fields have been set and if so zoom to the location, this is used when a user has selected a fault and then navigated to the map page.
    if (document.getElementById('lat').value != "" && document.getElementById('long').value != "") {
        const position = [];
        position[0] = document.getElementById('long').value;
        position[1] = document.getElementById('lat').value;

        //Create a marker to display the location of the fault.
        marker = new atlas.HtmlMarker({
            htmlContent: "<div><div class='marker'></div><div class='pulse'></div></div>",
            position: position,
            pixelOffset: [5, -18]
        });

        //Add the marker to the map.
        map.markers.add(marker);

        //Set the camera to the location of the fault.
        map.setCamera({
            // Center map on marker
            center: [Number(position[0]), Number(position[1])],
            // Set zoom to 18 to get close to marker
            zoom: 18,
            // Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland
            maxBounds: mapBounds,
        });
    }
}