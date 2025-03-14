﻿// CODE SUMMARY:
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

    CheckForSessionMarker();
}

//FUNCTION SUMMARY:
//This method is used for snapping a marker to the closest road on the map control.
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

                //Do a reverse geocode on the snapped coordinate to get the address of the location.
                reverseAddressSearch(closestCoord);
            }
        }
    }
}

// METHOD SUMMARY - This method is used for placing the marker for the fault on the map control
function placeMarker(position)
{
    // Clear any existing markers on the map, only 1 can be added at a time.
    map.markers.clear();

    // Create a marker for the fault location.
    marker = new atlas.HtmlMarker({
        htmlContent: "<div><div class='marker'></div><div class='pulse'></div></div>",
        position: position,
        pixelOffset: [5, -18]
    });

    // Add the marker to the map control.
    map.markers.add(marker);
}

// FUNCTION SUMMARY:
// This method is used for reverse geocoding the snapped location to get the address of the location.
function reverseAddressSearch(position) {

    // Use the Azure Maps SearchURL client to reverse geocode the snapped location to get the address of marker location.
    searchURL.searchAddressReverse(atlas.service.Aborter.timeout(3000), position, {
        view: 'Auto',
        radius: 7
    }).then(results => {

        //Get the features from the data of the marker location.
        var result = results.geojson.getFeatures();

        //Check if the marker is within Northern Ireland.
        if (result.features[0].properties.address.countrySubdivisionName == "Northern Ireland") {

            // Hide the selection error message if it is displayed.
            document.getElementById('selectionError').style.display = "none";

            // Place the marker on the map at the snapped location.
            placeMarker(position);

            // Get the road number, road name, town and county of the snapped location and store them in the form fields.
            if (result.features.length > 0 && result.features[0].properties && result.features[0].properties.address) {
                var roadNum = (result.features[0].properties.address.routeNumbers[0]);
                var roadName = (result.features[0].properties.address.streetName);
                var roadTown = (result.features[0].properties.address.localName);
                var roadCounty = (result.features[0].properties.address.countrySecondarySubdivision);
                document.getElementById('lat').value = position[1];
                document.getElementById('long').value = position[0];
                document.getElementById('roadNumber').value = roadNum;
                document.getElementById('roadName').value = roadName;
                document.getElementById('roadTown').value = roadTown;
                document.getElementById('roadCounty').value = roadCounty;
            } else {
                console.log("No address for that location!");
            }
        }
        // Display an error message if the marker is not within Northern Ireland.
        else {
            document.getElementById('selectionError').style.display = "block";

            // Hide the error message after 3 seconds.
            setTimeout(() => {
                document.getElementById('selectionError').style.display = "none";
            }, 3000);
        }
    });
}

// FUNCTION SUMMARY:
// This function is used to check if a session marker exists and if so place it on the map, needed for if a user navigates back to the page.
function CheckForSessionMarker()
{
    // Check if the session storage contains a marker.
    if (document.getElementById('lat').value != "" && document.getElementById('long').value != "")
    {
        // Get the lat/long of the marker from the session storage.
        const position = [];
        position[0] = document.getElementById('long').value;
        position[1] = document.getElementById('lat').value;

        // Create a marker for the fault location.
        marker = new atlas.HtmlMarker({
            htmlContent: "<div><div class='marker'></div><div class='pulse'></div></div>",
            position: position,
            pixelOffset: [5, -18]
        });

        // Add the marker to the map.
        map.markers.add(marker);

        // Set the camera to the marker location.
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