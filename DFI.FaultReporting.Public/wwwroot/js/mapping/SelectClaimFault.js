//CODE SUMMARY:
//This file controls the view faults map.A user can search for locations on the map using the autocomplete search,
//when they click on a suggestion returned by the autocomplete the map will zoom to that location. Pins are added to the map corresponding
//to each fault. If faults are close together a single marker will be used to represent the cluster of faults. When a user clicks
//a pin a popup is shown displaying information about a fault.

////Azure Maps Subscription Key - Needed to access the service
//const azureMapsSubscriptionKey = "tVwRA8vhqB9AvHiYgZa1muR90phLPrp6qzmJFvjqa0Q";

//Global variables used throughout code
var map, marker, searchURL, source, popup

////Define map boundaries - this will restrict how far a user can pan on the map control and also the search results returned in the autocomplete
//const mapBounds = [-8.3, 53.9, -5.3, 55.4]

//FUNCTION SUMMARY:
//This method is used for setting up the map control and adding event listeners to the map - uses the Azure Maps Web SDK.
//In this instance a data source for displaying faults is created and added to the map also.
function initFaultMap() {

    //Create new map control
    map = new atlas.Map('faultsMap', {
        //Center map on Northern Ireland
        center: [-6.8, 54.65],
        //Set zoom to 3 to show as much of Northern Ireland as possible
        zoom: 3,
        //Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland
        maxBounds: mapBounds,
        //Set lanuage to GB for better localisation
        language: 'en-GB',
        //Set auth
        authOptions: {
            authType: 'subscriptionKey',
            subscriptionKey: azureMapsSubscriptionKey
        },
        style: 'road'
    });

    //Add full screen control to the map.
    map.controls.add(new atlas.control.FullscreenControl(), {
        position: 'bottom-right',
        style: 'auto'
    });

    //Add zoom control to the map.
    map.controls.add(new atlas.control.ZoomControl(), {
        position: 'bottom-right'
    });

    //Add the style control to the map.
    map.controls.add(new atlas.control.StyleControl(), {
        position: 'bottom-right',
        mapStyles: 'all'
    });

    //Use MapControlCredential to share authentication between a map control and the service module
    var pipeline = atlas.service.MapsURL.newPipeline(new atlas.service.MapControlCredential(map));

    //Create an instance of the SearchURL client
    searchURL = new atlas.service.SearchURL(pipeline);

    //Wait until map is ready to ensure data source is successully added..
    map.events.add('ready', function () {
        //Create a new dictionary to store the geo json representations of faults.
        const geoJsonFaults = [];

        //Loop over each of the faults in the session.
        faults.forEach(function (fault) {

            //Get the position of a fault so it can be used as the point for the fault geo json feature.
            var position = [];
            position[0] = fault.longitude;
            position[1] = fault.latitude;

            //Create a geo json representation of a fault.
            var geoJsonFault = new atlas.data.Feature(new atlas.data.Point(position), {
                "id": fault.id,
            });

            //Add the geo json representation of a faul to the dictionary.
            geoJsonFaults.push(geoJsonFault);
        });

        console.log(geoJsonFaults);

        //Create the popup to be used to display the details of a fault when the user clicks a marker.
        popup = new atlas.Popup({
            pixelOffset: [0, -20],
            closeButton: false
        });

        //Create a new data source for the map.
        source = new atlas.source.DataSource(null, {
            //Set the cluster option to true to enable fault that are close together to be represented by a single marker when zoomed out.
            cluster: true
        });

        //Add the source to the map.
        map.sources.add(source);

        //Create the marker layer to be used to display faults.
        markerLayer = new atlas.layer.HtmlMarkerLayer(source, null, {
            markerCallback: (id, position, properties) => {
                //Marker will represent a cluster of faults close together.
                if (properties.cluster) {
                    console.log("Properties:");
                    console.log(properties);

                    //Return a created marker for the cluster.
                    return new atlas.HtmlMarker({
                        htmlContent: "<div><div class='marker'></div><div class='pulse'></div></div>",
                        position: position,
                        pixelOffset: [5, -18]
                    });
                }

                console.log("Properties:");
                console.log(properties);

                //Use a promise to create a marker.
                return Promise.resolve(new atlas.HtmlMarker({
                    htmlContent: "<div><div class='marker'></div><div class='pulse'></div></div>",
                    pixelOffset: [5, -18],
                    position: position
                }));
            }
        });

        //Add click event to map when marker is clicked to display popup.
        map.events.add('click', markerLayer, markerClicked);

        //Add the created marker layer to the map.
        map.layers.add(markerLayer);

        //Import the created geo JSON data to the maps source data.
        source.add(geoJsonFaults);
    });

    if (hasFault)
    {
        CheckForSessionClaimFaultMarker();
    }
}

//FUNCTION SUMMARY:
//This method is used defining what happens when a marker is clicked.
function markerClicked(e) {
    //Declare content variable to store information about faults.
    var content;

    //Get the marker that was clicked from the target of the click.
    var marker = e.target;

    console.log("Marker Properties:");
    console.log(marker.properties);

    //The clicked marker represents a cluster of faults.
    if (marker.properties.cluster) {

        //Set the content value to the number of faults that are represented by the cluster marker.
        content = `${marker.properties.point_count_abbreviated} faults`;

        //Setup popup.
        popup.setOptions({
            content: `<div style="padding:10px;"><strong>${content}</strong></div>`,
            position: marker.getOptions().position,
            closeButton: true
        });
    }
    //The clicked marker represents a single fault.
    else {


        var position = marker.getOptions().position;


        document.getElementById("faultID").value = marker.properties.id;
        document.getElementById('lat').value = position[1];
        document.getElementById('long').value = position[0];



        //Setup popup.
        popup.setOptions({
            content: `<div style="padding:10px;">
                              <p><strong>Selected</strong></p>
                           </div>`,
            position: marker.getOptions().position,
            closeButton: true
        });
    }

    //Show the popup.
    popup.open(map);
}

//FUNCTION SUMMARY:
//This method is used defining what happens when close button on a marker is clicked.
function hidePopup() {

    //Hide the popup.
    popup.close();
}


function CheckForSessionClaimFaultMarker() {
    if (claim != null) {
        if (claim.incidentLocationLatitude != null && claim.incidentLocationLongitude) {
            const position = [];
            position[0] = claim.incidentLocationLongitude;
            position[1] = claim.incidentLocationLatitude;

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


    if (document.getElementById('lat').value != "" && document.getElementById('long').value != "") {
        const position = [];
        position[0] = document.getElementById('long').value;
        position[1] = document.getElementById('lat').value;

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