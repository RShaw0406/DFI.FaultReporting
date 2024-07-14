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
//In this instance a data source for displaying reapirs is created and added to the map also.
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

        navigator.geolocation.getCurrentPosition(SetStartPosition);

        //Wait until the map resources are ready.
        map.events.add('ready', function () {

            //Create a data source to store the data in.
            datasource = new atlas.source.DataSource();
            map.sources.add(datasource);

            //Add layers fro rendering data in the data source.
            map.layers.add([
                //Render linestring data using a line layer.
                new atlas.layer.LineLayer(datasource, null, {
                    strokeColor: 'red',
                    strokeWidth: 5
                }),

                //Render point data using a symbol layer.
                new atlas.layer.SymbolLayer(datasource, null, {
                    textOptions: {
                        textField: ['get', 'title'],
                        offset: [0, -1.2],
                        color: 'white'
                    },
                    filter: ['any', ['==', ['geometry-type'], 'Point'], ['==', ['geometry-type'], 'MultiPoint']] //Only render Point or MultiPoints in this layer.
                })
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

function SetStartPosition(position) {

    var startPosition = [];
    startPosition[0] = position.coords.longitude;
    startPosition[1] = position.coords.latitude;

    routePoints.push(startPosition)
}

function GenerateRoute(optimized) {

    //Remove any previously calculated route information.
    if (routeLine) {
        datasource.remove(routeLine);
        routeLine = null;
    }

    //Create request to calculate a route in the order in which the routePoints are provided.
    var requestUrl = restRoutingRequestUrl.replace('{query}', routePointQuery);

    if (optimized) {
        requestUrl += '&computeBestOrder=true';
    }

    //Proces the request.
    processRequest(requestUrl).then(r => {
        PlotRoute(r.routes[0]);

        var output = 'Distance: ' + Math.round(r.routes[0].summary.lengthInMeters / 10) / 100 + ' km<br/>';

        if (optimized) {
            var pinOrder = ['0'];

            for (var i = 0; i < r.optimizedWaypoints.length; i++) {
                //Increase index by one to account for origin index.
                pinOrder.push(r.optimizedWaypoints[i].optimizedIndex + 1);
            }

            //Add the desintation index to the end.
            pinOrder.push(routePoints.length - 1);

            output += 'Waypoint Order: ' + pinOrder.join(', ');
        } else {
            output += 'Waypoint Order: ' + defaultOrder.join(', ');
        }
    });
}

function PlotRoute(route) {
    var routeCoordinates = [];

    for (var legIndex = 0; legIndex < route.legs.length; legIndex++) {
        var leg = route.legs[legIndex];

        //Convert the route point data into a format that the map control understands.
        var legCoordinates = leg.points.map(function (point) {
            return [point.longitude, point.latitude];
        });

        //Combine the route point data for each route leg together to form a single path.
        routeCoordinates = routeCoordinates.concat(legCoordinates);
    }

    //Create a LineString from the route path points and add it to the data source.
    routeLine = new atlas.Shape(new atlas.data.LineString(routeCoordinates));
    datasource.add(routeLine);
}