//CODE SUMMARY:
//This file controls the review map on step 4 of the report a fault process and the AddReport page. When the map renders a pin is placed on the map to
//display the location that the user selected for the fault.

//FUNCTION SUMMARY:
//This method is used for setting up the map control and adding event listeners to the map - uses the Azure Maps Web SDK.
function initReviewMap() {

    //Azure Maps Subscription Key - Needed to access the service
    const azureMapsSubscriptionKey = "tVwRA8vhqB9AvHiYgZa1muR90phLPrp6qzmJFvjqa0Q";

    //Global variables used throughout code
    var map, marker, searchURL

    //Define map boundaries - this will restrict how far a user can pan on the map control and also the search results returned in the autocomplete
    const mapBounds = [-8.3, 53.9, -5.3, 55.4]

    //Create the position dictionary by getting the values of the lat/long hidden fields on the page.
    const position = [];
    position[0] = document.getElementById('long').value;
    position[1] = document.getElementById('lat').value;

    //Create new map control
    map = new atlas.Map('reviewMap', {
        //Center map on marker
        center: [Number(position[0]), Number(position[1])],
        //Set zoom to 18 to get close to marker
        zoom: 18,
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

    //Create a marker to display the location of the fault.
    marker = new atlas.HtmlMarker({
        htmlContent: "<div><div class='marker'></div><div class='pulse'></div></div>",
        position: position,
        pixelOffset: [5, -18]
    });

    //Add the marker to the map.
    map.markers.add(marker);
}