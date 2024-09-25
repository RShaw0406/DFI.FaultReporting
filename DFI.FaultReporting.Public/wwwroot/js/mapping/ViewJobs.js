//CODE SUMMARY:
//This file controls the contractors jobs map. A user can search for locations on the map using the autocomplete search,
//when they click on a suggestion returned by the autocomplete the map will zoom to that location. Pins are added to the map corresponding
//to each assigned repair. If repairs are close together a single marker will be used to represent the cluster of repairs. When a user clicks
//a pin a popup is shown displaying information about a repairs.

//Azure Maps Subscription Key - Needed to access the service
const azureMapsSubscriptionKey = "tVwRA8vhqB9AvHiYgZa1muR90phLPrp6qzmJFvjqa0Q";

//Global variables used throughout code
var map, marker, searchURL, source, popup

//Define map boundaries - this will restrict how far a user can pan on the map control and also the search results returned in the autocomplete
const mapBounds = [-8.3, 53.9, -5.3, 55.4]

//FUNCTION SUMMARY:
//This method is used for setting up the map control and adding event listeners to the map - uses the Azure Maps Web SDK.
//In this instance a data source for displaying reapirs is created and added to the map also.
function initJobsMap() {
    //Find map on page
    var jobsMap = document.getElementById('jobsMap');

    if (jobsMap != null) {
        //Create new map control
        map = new atlas.Map('jobsMap', {
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
            //Create a new dictionary to store the geo json representations of repairs.
            const geoJsonRepairs = [];

            //Loop over each of the repairs in the session.
            repairs.forEach(function (repair) {

                //Declare variables to be used for displaying in popup to ensure fresh start each time a repair is looped over.
                var status, statusID, repairFault, repairFaultType, repairFaultPriority, repairFaultStatus, road, position;

                //Loop over each of the statuses in the session.
                repairStatuses.forEach(function (repairStatus) {

                    //Repair status is related to the current repair.
                    if (repair.repairStatusID == repairStatus.id) {
                        //Set the status to be used for displaying in popup.
                        status = repairStatus.repairStatusDescription;
                        statusID = repairStatus.id;
                    }
                });

                //Loop over each of the faults in the session.
                faults.forEach(function (fault) {
                    //Fault is related to the current repair.
                    if (repair.faultID == fault.id) {
                        //Set the repairFault to be used for displaying in popup.
                        repairFault = fault;
                    }

                    //Loop over each of the types in the session.
                    faultTypes.forEach(function (faultType) {

                        //Fault type is related to the current fault.
                        if (fault.faultTypeID == faultType.id) {
                            //Set the repairFaultType to be used for displaying in popup.
                            repairFaultType = faultType.faultTypeDescription;
                        }
                    });

                    //Loop over each of the priorities in the session.
                    faultPriorities.forEach(function (faultPriority) {

                        //Fault priority is related to the current fault.
                        if (fault.faultPriorityID == faultPriority.id) {
                            //Set the repairFaultPriority to be used for displaying in popup.
                            repairFaultPriority = faultPriority.faultPriorityDescription;
                        }
                    });

                    //Loop over each of the statuses in the session.
                    faultStatuses.forEach(function (faultStatus) {

                        //Fault status is related to the current fault.
                        if (fault.faultStatusID == faultStatus.id) {
                            //Set the repairFaultStatus to be used for displaying in popup.
                            repairFaultStatus = faultStatus.faultStatusDescription;
                        }
                    });




                    //Create a new string of road details to display in popup and clean out any "undefined".
                    road = fault.roadNumber + ", " + fault.roadName + ", " + fault.roadTown + ", " + fault.RoadCounty;
                    road = road.replaceAll(", undefined", "");
                    road = road.replaceAll("undefined, ", "");

                });

                //Get the position of a fault so it can be used as the point for the repair geo json feature.
                position = []
                position[0] = repairFault.longitude;
                position[1] = repairFault.latitude;

                //Create a geo json representation of a repair.
                var geoJsonRepair = new atlas.data.Feature(new atlas.data.Point(position), {
                    "id": repair.id,
                    "statusID": statusID,
                    "status": status,
                    "targetDate": new Date(repair.repairTargetDate).toLocaleDateString(),
                    "actualDate": new Date(repair.actualRepairDate).toLocaleDateString(),
                    "fault": repairFault,
                    "faultType": repairFaultType,
                    "faultPriority": repairFaultPriority,
                    "faultStatus": repairFaultStatus,
                    "road": road,
                });

                if (!geoJsonRepairs.map(e => e.properties.id).includes(geoJsonRepair.properties.id)) {
                    //Add the geo json representation of a fault to the dictionary.
                    geoJsonRepairs.push(geoJsonRepair);
                }

            });

            //Create the popup to be used to display the details of a repair when the user clicks a marker.
            popup = new atlas.Popup({
                pixelOffset: [0, -20],
                closeButton: false
            });

            //Create a new data source for the map.
            source = new atlas.source.DataSource(null, {
                //Set the cluster option to true to enable repair that are close together to be represented by a single marker when zoomed out.
                cluster: true
            });

            //Add the source to the map.
            map.sources.add(source);

            //Create the marker layer to be used to display repairs.
            markerLayer = new atlas.layer.HtmlMarkerLayer(source, null, {
                markerCallback: (id, position, properties) => {
                    //Marker will represent a cluster of repairs close together.
                    if (properties.cluster) {

                        //Return a created marker for the cluster.
                        return new atlas.HtmlMarker({
                            htmlContent: "<div><div class='marker'></div><div class='pulse'></div></div>",
                            position: position,
                            pixelOffset: [5, -18]
                        });
                    }

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
            source.add(geoJsonRepairs);
        });
    }
}

//FUNCTION SUMMARY:
//This method is used defining what happens when a marker is clicked.
function markerClicked(e) {
    //Declare content variable to store information about repairs.
    var content;

    //Get the marker that was clicked from the target of the click.
    var marker = e.target;

    //The clicked marker represents a cluster of repairs.
    if (marker.properties.cluster) {

        //Set the content value to the number of repairs that are represented by the cluster marker.
        content = `${marker.properties.point_count_abbreviated} repairs`;

        //Setup popup.
        popup.setOptions({
            content: `<div style="padding:10px;"><strong>${content}</strong></div>`,
            position: marker.getOptions().position,
            closeButton: true
        });
    }
    //The clicked marker represents a single repair.
    else {

        //Get the data required for displaying in the popup.
        var contentRepairStatus = marker.properties.status;
        var contentRepairTargetDate = marker.properties.targetDate;
        var contentActualRepairDate = marker.properties.actualDate;
        var contentFaultType = marker.properties.faultType;
        var contentFaultPriority = marker.properties.faultPriority;
        var contenFaultStatus = marker.properties.faultStatus;
        var contentRoad = marker.properties.road;

        //Create an href value for the "Edit details" link and add the ID of the repair as a query parameter
        var urlEdit = "/Jobs/EditJob/?ID=" + marker.properties.id;

        //Create an href value for the "View photos" link and add the ID of the repair as a query parameter
        var urlPhotos = "/Jobs/JobImages/?ID=" + marker.properties.id;

        if (marker.properties.statusID == 3) {


            if (marker.properties.actualDate <= marker.properties.targetDate) {

                //Setup popup.
                popup.setOptions({
                    content: `<div style="padding:10px;">
                              <p><strong>Status:</strong> ${contentRepairStatus}</p>
                              <span class="badge bg-success">Target met</span>
                              <p><strong>Target date:</strong> ${contentRepairTargetDate}</p>
                              <p><strong>Actual date:</strong> ${contentActualRepairDate}</p>
                              <p><strong>Fault type:</strong> ${contentFaultType}</p>
                              <p><strong>Priority:</strong> ${contentFaultPriority}</p>
                              <p><strong>Location:</strong> ${contentRoad}</p>
                              <a id="linkJobImages" href="${urlPhotos}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-magnifying-glass-plus"></i> View photos</a>
                           </div>`,
                    position: marker.getOptions().position,
                    closeButton: true
                });
            }
            else
            {

                //Setup popup.
                popup.setOptions({
                    content: `<div style="padding:10px;">
                              <p><strong>Status:</strong> ${contentRepairStatus}</p>
                              <span class="badge bg-danger">Target not met</span>
                              <p><strong>Target date:</strong> ${contentRepairTargetDate}</p>
                              <p><strong>Actual date:</strong> ${contentActualRepairDate}</p>
                              <p><strong>Fault type:</strong> ${contentFaultType}</p>
                              <p><strong>Priority:</strong> ${contentFaultPriority}</p>
                              <p><strong>Location:</strong> ${contentRoad}</p>
                              <a id="linkJobImages" href="${urlPhotos}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-magnifying-glass-plus"></i> View photos</a>
                           </div>`,
                    position: marker.getOptions().position,
                    closeButton: true
                });
            }

        }
        else {
            //Setup popup.
            popup.setOptions({
                content: `<div style="padding:10px;">
                              <p><strong>Status:</strong> ${contentRepairStatus}</p>
                              <p><strong>Target date:</strong> ${contentRepairTargetDate}</p>
                              <p><strong>Fault type:</strong> ${contentFaultType}</p>
                              <p><strong>Priority:</strong> ${contentFaultPriority}</p>
                              <p><strong>Location:</strong> ${contentRoad}</p>
                              <a id="linkEditJob" href="${urlEdit}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-pen-to-square"></i> Edit details</a>
                              <a id="linkJobImages" href="${urlPhotos}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-magnifying-glass-plus"></i> View photos</a>
                           </div>`,
                position: marker.getOptions().position,
                closeButton: true
            });
        }
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