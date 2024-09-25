//CODE SUMMARY:
//This file controls the route plotting functionality on the map control.
//The user can select a number of jobs and the system will calculate the best route between the jobs.
//Route points are ordered based on the priority of the jobs.
//The route will be displayed on the map control and the user can choose to optimise the route order or not.

//Azure Maps Subscription Key - Needed to access the service
const azureMapsSubscriptionKey = "tVwRA8vhqB9AvHiYgZa1muR90phLPrp6qzmJFvjqa0Q";

var restRoutingRequestUrl = 'https://{azMapsDomain}/route/directions/json?api-version=1.0&query={query}&routeRepresentation=polyline&travelMode=car&view=Auto';

//Global variables used throughout code
var map, marker, searchURL, source, popup, routePointQuery, routeLine, defaultOrder;

var routePoints = [];

//Define map boundaries - this will restrict how far a user can pan on the map control and also the search results returned in the autocomplete
const mapBounds = [-8.3, 53.9, -5.3, 55.4]

//FUNCTION SUMMARY:
//This method is used for setting up the map control and adding event listeners to the map - uses the Azure Maps Web SDK.
//In this instance a data source for displaying selected repar jobs is created and added to the map also.
function initRouteMap() {

    //Find map on page
    var routeMap = document.getElementById('routeMap');

    if (routeMap != null) {
        //Create new map control
        map = new atlas.Map('routeMap', {
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

        faults.forEach(function (fault) {
            position = []
            position[0] = fault.longitude;
            position[1] = fault.latitude;
            routePoints.push(position)
        });


        //Wait until the map resources are ready.
        map.events.add('ready', function () {

            //Create a data source to store the data in.
            datasource = new atlas.source.DataSource();
            map.sources.add(datasource);

            var markerLayer;

            //Add layers fro rendering data in the data source.
            map.layers.add([
                //Render linestring data using a line layer.
                new atlas.layer.LineLayer(datasource, null, {
                    strokeColor: 'red',
                    strokeWidth: 5
                }),

                markerLayer = new atlas.layer.HtmlMarkerLayer(datasource, null, {
                    markerCallback: (id, position, properties) => {
                        //Marker will represent a cluster of repairs close together.
                        if (properties.cluster) {
                            console.log("Properties:");
                            console.log(properties);

                            //Return a created marker for the cluster.
                            return new atlas.HtmlMarker({
                                text: properties.title,
                                color: 'blue',
                                position: position
                            });
                        }

                        if (properties.title == "0") {
                            //Return a created marker for the cluster.
                            return new atlas.HtmlMarker({
                                color: 'green',
                                position: position
                            });
                        }
                        else
                        {
                            //Use a promise to create a marker.
                            return Promise.resolve(new atlas.HtmlMarker({
                                text: properties.title,
                                color: 'blue',
                                position: position
                            }));
                        }
                    }
                }),
            ]);

            //Add routePoints to the map and build the waypoint query string.
            var points = [];
            var query = [];
            defaultOrder = [];

            for (var i = 0; i < routePoints.length; i++) {
                points.push(new atlas.data.Feature(new atlas.data.Point(routePoints[i]), {
                    title: i + ''
                }));

                query.push(routePoints[i][1] + ',' + routePoints[i][0]);
                defaultOrder.push(i);
            }

            //Add the points to the data source.
            datasource.add(points);

            //Create the waypoint query string value to be used in the routing requests.
            routePointQuery = query.join(':');
        });

    }
}

//FUNCTION SUMMARY:
//This function is used to get the users current location and set the start position of the route.
function GetUserLocation() {

    const geoLocationOptions = {
        enableHighAccuracy: true,
        timeout: 300000
    };

    //Get the users current location.
    navigator.geolocation.getCurrentPosition(SetStartPosition, GeoLocationError, geoLocationOptions);
}

//FUNCTION SUMMARY:
//This function is executed when an error occurs when trying to get the users location.
function GeoLocationError(error) {
    console.log('Error occurred. Error code: ' + error.code + ' Error message: ' + error.message);
}

//FUNCTION SUMMARY:
//This function is used to set the start position of the route.
function SetStartPosition(position) {

    var startPosition = [];
    startPosition[0] = position.coords.longitude;
    startPosition[1] = position.coords.latitude;

    routePoints.push(startPosition)

    //Initalise the map control after the start position has been set.
    initRouteMap();
}

//FUNCTION SUMMARY:
//This function is used to calculate a route between the selected jobs.
function GenerateRoute(optimized) {

    //Remove any previously calculated route information.
    if (routeLine) {
        datasource.remove(routeLine);
        routeLine = null;
    }

    //Create request to calculate a route in the order in which the routePoints are provided (based on priority).
    var requestUrl = restRoutingRequestUrl.replace('{query}', routePointQuery);

    if (optimized) {
        requestUrl += '&computeBestOrder=true';
    }

    //Proces the request.
    processRequest(requestUrl).then(r => {
        PlotRoute(r.routes[0]);

        //If the route has been optimized, update the pin order to reflect the new order based on the most optimal route.
        if (optimized) {
            var pinOrder = ['0'];

            for (var i = 0; i < r.optimizedWaypoints.length; i++) {
                //Account for starting point by increasing the index by 1.
                pinOrder.push(r.optimizedWaypoints[i].optimizedIndex + 1);
            }

            //Add the end point to the pin order.
            pinOrder.push(routePoints.length - 1);
        }
    });
}

//FUNCTION SUMMARY:
//This function is used to plot the route on the map control.
function PlotRoute(route) {
    var routeCoordinates = [];

    for (var legIndex = 0; legIndex < route.legs.length; legIndex++) {
        var leg = route.legs[legIndex];

        //Convert the route point data into lat long coordinates.
        var legCoordinates = leg.points.map(function (point) {
            return [point.longitude, point.latitude];
        });

        //Combine the route points into a single array to create the line.
        routeCoordinates = routeCoordinates.concat(legCoordinates);
    }

    //Create a line shape to represent the route.
    routeLine = new atlas.Shape(new atlas.data.LineString(routeCoordinates));
    datasource.add(routeLine);
}