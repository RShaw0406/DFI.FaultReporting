//CODE SUMMARY:
//This file controls the heatmap report. The heatmap is used to display heat blooms depending on the number of reports for a fault. 
//The more reports a fault has the more intense the heat bloom will be.


//Azure Maps Subscription Key - Needed to access the service
const azureMapsSubscriptionKey = "tVwRA8vhqB9AvHiYgZa1muR90phLPrp6qzmJFvjqa0Q";

var map, datasource;

//Define map boundaries - this will restrict how far a user can pan on the map control and also the search results returned in the autocomplete
const mapBounds = [-8.3, 53.9, -5.3, 55.4]

function initHeatMap() {
    //Initialize a map instance.
    map = new atlas.Map('heatMap', {
        //Center map on Northern Ireland
        center: [-6.8, 54.65],
        //Set zoom to 3 to show as much of Northern Ireland as possible
        zoom: 3,
        //Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland
        maxBounds: mapBounds,
        //Set lanuage to GB for better localisation
        language: 'en-GB',

        authOptions: {
            authType: 'subscriptionKey',
            subscriptionKey: azureMapsSubscriptionKey
        },

        view: 'Auto'
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

    //Wait until the map resources are ready.
    map.events.add('ready', function () {

        //Create a new dictionary to store the geo json representations of faults.
        const geoJsonFaults = [];

        //Loop over each of the faults in the session.
        faults.forEach(function (fault) {

            //Declare variables to be used for displaying in popup to ensure fresh start each time a fault is looped over.
            var type, priority, status;
            let faultReports = 0;

            //Loop over each of the types in the session.
            faultTypes.forEach(function (faultType) {

                //Type is related to the current fault.
                if (fault.faultTypeID == faultType.id) {
                    //Set the type to be used for displaying in popup.
                    type = faultType.faultTypeDescription;
                }
            });

            //Loop over each of the priorities in the session.
            faultPriorities.forEach(function (faultPriority) {

                //Priority is related to the current fault.
                if (fault.faultPriorityID == faultPriority.id) {
                    //Set the priority to be used for displaying in popup.
                    priority = faultPriority.faultPriorityDescription;
                }
            });

            //Loop over each of the statuses in the session.
            faultStatuses.forEach(function (faultStatus) {

                //Status is related to the current fault.
                if (fault.faultStatusID == faultStatus.id) {
                    //Set the status description to be used for displaying in popup.
                    status = faultStatus.faultStatusDescription;
                }
            });

            //Loop over each of the reports in the session.
            reports.forEach(function (report) {

                //Report is related to the current fault.
                if (report.faultID == fault.id) {

                    //Increment the report counter to be used for displaying in popup.
                    faultReports++;
                }
            });

            //Get the position of a fault so it can be used as the point for the fault geo json feature.
            var position = [];
            position[0] = fault.longitude;
            position[1] = fault.latitude;

            //Create a new string of road details to display in popup and clean out any "undefined".
            var road = fault.roadNumber + ", " + fault.roadName + ", " + fault.roadTown + ", " + fault.RoadCounty;
            console.log(road);
            road = road.replaceAll(", undefined", "");
            road = road.replaceAll("undefined, ", "");

            //Create a geo json representation of a fault.
            var geoJsonFault = new atlas.data.Feature(new atlas.data.Point(position), {
                "id": fault.id,
                "type": type,
                "status": status,
                "priority": priority,
                "road": road,
                "reportCount": faultReports
            });

            //Add the geo json representation of a faul to the dictionary.
            geoJsonFaults.push(geoJsonFault);
        });


        //Create a data source and add it to the map.
        datasource = new atlas.source.DataSource();
        map.sources.add(datasource);

        ////Load a data set of points, in this case earthquake data from the USGS.
        //datasource.importDataFromUrl('https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_week.geojson');

        //Import the created geo JSON data to the maps source data.
        datasource.add(geoJsonFaults);

        //Create a heatmap and add it to the map.
        map.layers.add(new atlas.layer.HeatMapLayer(datasource, null, {
            radius: 10,
            opacity: 0.8,
            weight: 1
        }), 'labels');
    });
}