﻿//CODE SUMMARY:
//This file controls the view faults map.A user can search for locations on the map using the autocomplete search,
//when they click on a suggestion returned by the autocomplete the map will zoom to that location. Pins are added to the map corresponding
//to each fault. If faults are close together a single marker will be used to represent the cluster of faults. When a user clicks
//a pin a popup is shown displaying information about a fault.

//Azure Maps Subscription Key - Needed to access the service
const azureMapsSubscriptionKey = "tVwRA8vhqB9AvHiYgZa1muR90phLPrp6qzmJFvjqa0Q";

//Global variables used throughout code
var map, marker, searchURL, source, popup

//Define map boundaries - this will restrict how far a user can pan on the map control and also the search results returned in the autocomplete
const mapBounds = [-8.3, 53.9, -5.3, 55.4]

//FUNCTION SUMMARY:
//This method is used for setting up the map control and adding event listeners to the map - uses the Azure Maps Web SDK.
//In this instance a data source for displaying faults is creates and added to the map also.
function initViewMap() {

    //Create new map control.
    map = new atlas.Map('faultsMap', {
        //Center map on Northern Ireland.
        center: [-6.8, 54.65],
        //Set zoom to 3 to show as much of Northern Ireland as possible.
        zoom: 3,
        //Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland.
        maxBounds: mapBounds,
        //Set lanuage to GB for better localisation.
        language: 'en-GB',
        //Set auth.
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

    //Use MapControlCredential to share authentication between a map control and the service module.
    var pipeline = atlas.service.MapsURL.newPipeline(new atlas.service.MapControlCredential(map));

    //Create an instance of the SearchURL client.
    searchURL = new atlas.service.SearchURL(pipeline);

    //Wait until map is ready to ensure data source is successully added.
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
        source.add(geoJsonFaults);
    });
}

//FUNCTION SUMMARY:
//This method is used defining what happens when a marker is clicked.
function markerClicked(e) {
    //Declare content variable to store information about faults.
    var content;

    //Get the marker that was clicked from the target of the click.
    var marker = e.target;

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

        //Get the data required for displaying in the popup.
        var contentType = marker.properties.type;
        var contentPriority = marker.properties.priority;
        var contentStatus = marker.properties.status;
        var contentRoad = marker.properties.road;
        var contentReports = marker.properties.reportCount;

        //Set the URLs for the different actions that can be taken on a fault.
        var urlStaff = "/Faults/AssignStaff/?ID=" + marker.properties.id;
        var urlReports = "/Faults/ReportDetails/?ID=" + marker.properties.id;
        var urlEdit = "/Faults/EditFault/?ID=" + marker.properties.id;
        var urlRepair = "/Faults/ScheduleRepair/?ID=" + marker.properties.id;


        //Setup popup.
        if (readWrite == true) {

            //Declare variables to store whether a fault has staff or repairs.
            var hasStaff = false;
            var hasRepair = false;

            //Loop over each of the faults in the session.
            faults.forEach(function (fault) {

                if (fault.id == marker.properties.id) {
                    //Loop over each of the staff in the session
                    staff.forEach(function (staffMember) {

                        //Check if the fault has staff assigned to it.
                        if (fault.staffID == staffMember.id) {
                            hasStaff = true;
                        };
                    });

                    //Loop over each of the repairs in the session.
                    repairs.forEach(function (repair) {
                        //Check if the fault has repairs scheduled.
                        if (repair.faultID == fault.id) {
                            hasRepair = true;
                        };
                    });
                }
            });

            //If a fault has staff create popup without assign staff button.
            if (hasStaff) {
                popup.setOptions({
                    content: `<div style="padding:10px;">
                              <p><strong>Type:</strong> ${contentType}</p>
                              <p><strong>Priority:</strong> ${contentPriority}</p>
                              <p><strong>Status:</strong> ${contentStatus}</p>
                              <p><strong>Location:</strong> ${contentRoad}</p>
                              <p><strong>Reports:</strong> ${contentReports}</p>
                              <a id="linkViewReports" href="${urlReports}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-magnifying-glass-plus"></i> View reports</a>
                              <br />
                              <br />
                              <a id="linkEditDetails" href="${urlEdit}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-pen-to-square"></i> Edit details</a>
                              <a id="linkScheduleRepair" href="${urlRepair}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-person-digging"></i> Schedule repair</a>
                           </div>`,
                    position: marker.getOptions().position,
                    closeButton: true
                });
            }

            //If a fault has repairs create popup without schedule repair button.
            if (hasRepair) {
                popup.setOptions({
                    content: `<div style="padding:10px;">
                              <p><strong>Type:</strong> ${contentType}</p>
                              <p><strong>Priority:</strong> ${contentPriority}</p>
                              <p><strong>Status:</strong> ${contentStatus}</p>
                              <p><strong>Location:</strong> ${contentRoad}</p>
                              <p><strong>Reports:</strong> ${contentReports}</p>
                              <a id="linkViewReports" href="${urlReports}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-magnifying-glass-plus"></i> View reports</a>
                              <a id="linkAssignStaff" href="${urlStaff}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-user-plus"></i> Assign staff</a>
                              <br />
                              <br />
                              <a id="linkEditDetails" href="${urlEdit}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-pen-to-square"></i> Edit details</a>
                           </div>`,
                    position: marker.getOptions().position,
                    closeButton: true
                });
            }

            //If a fault has staff and repairs create popup without assign staff or schedule repair buttons.
            if (hasStaff && hasRepair)
            {
                popup.setOptions({
                    content: `<div style="padding:10px;">
                              <p><strong>Type:</strong> ${contentType}</p>
                              <p><strong>Priority:</strong> ${contentPriority}</p>
                              <p><strong>Status:</strong> ${contentStatus}</p>
                              <p><strong>Location:</strong> ${contentRoad}</p>
                              <p><strong>Reports:</strong> ${contentReports}</p>
                              <a id="linkViewReports" href="${urlReports}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-magnifying-glass-plus"></i> View reports</a>
                              <a id="linkEditDetails" href="${urlEdit}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-pen-to-square"></i> Edit details</a>
                           </div>`,
                    position: marker.getOptions().position,
                    closeButton: true
                });
            }

            //If a fault has no staff or repairs create popup with all buttons.
            if (!hasStaff && !hasRepair)
            {
                popup.setOptions({
                    content: `<div style="padding:10px;">
                              <p><strong>Type:</strong> ${contentType}</p>
                              <p><strong>Priority:</strong> ${contentPriority}</p>
                              <p><strong>Status:</strong> ${contentStatus}</p>
                              <p><strong>Location:</strong> ${contentRoad}</p>
                              <p><strong>Reports:</strong> ${contentReports}</p>
                              <a id="linkViewReports" href="${urlReports}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-magnifying-glass-plus"></i> View reports</a>
                              <a id="linkAssignStaff" href="${urlStaff}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-user-plus"></i> Assign staff</a>
                              <br />
                              <br />
                              <a id="linkEditDetails" href="${urlEdit}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-pen-to-square"></i> Edit details</a>
                              <a id="linkScheduleRepair" href="${urlRepair}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-person-digging"></i> Schedule repair</a>
                           </div>`,
                    position: marker.getOptions().position,
                    closeButton: true
                });
            }

            //If a fault has no repairs and  staff create popup with all buttons except assign staff button.
            if (!hasRepair && hasStaff)
            {
                popup.setOptions({
                    content: `<div style="padding:10px;">
                              <p><strong>Type:</strong> ${contentType}</p>
                              <p><strong>Priority:</strong> ${contentPriority}</p>
                              <p><strong>Status:</strong> ${contentStatus}</p>
                              <p><strong>Location:</strong> ${contentRoad}</p>
                              <p><strong>Reports:</strong> ${contentReports}</p>
                              <a id="linkViewReports" href="${urlReports}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-magnifying-glass-plus"></i> View reports</a>
                              <br />
                              <br />
                              <a id="linkEditDetails" href="${urlEdit}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-pen-to-square"></i> Edit details</a>
                              <a id="linkScheduleRepair" href="${urlRepair}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-person-digging"></i> Schedule repair</a>
                           </div>`,
                    position: marker.getOptions().position,
                    closeButton: true
                });
            }

            //If a fault has repairs and no staff create popup with all buttons except schedule repair button.
            if (hasRepair && !hasStaff)
            {
                popup.setOptions({
                    content: `<div style="padding:10px;">
                              <p><strong>Type:</strong> ${contentType}</p>
                              <p><strong>Priority:</strong> ${contentPriority}</p>
                              <p><strong>Status:</strong> ${contentStatus}</p>
                              <p><strong>Location:</strong> ${contentRoad}</p>
                              <p><strong>Reports:</strong> ${contentReports}</p>
                              <a id="linkViewReports" href="${urlReports}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-magnifying-glass-plus"></i> View reports</a>
                              <br />
                              <br />
                              <a id="linkEditDetails" href="${urlEdit}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-pen-to-square"></i> Edit details</a>
                           </div>`,
                    position: marker.getOptions().position,
                    closeButton: true
                });
            }
        }
        //If user is read only create popup with no buttons.
        else
        {
            popup.setOptions({
                content: `<div style="padding:10px;">
                              <p><strong>Type:</strong> ${contentType}</p>
                              <p><strong>Priority:</strong> ${contentPriority}</p>
                              <p><strong>Status:</strong> ${contentStatus}</p>
                              <p><strong>Location:</strong> ${contentRoad}</p>
                              <p><strong>Reports:</strong> ${contentReports}</p>
                              <a id="linkViewReports" href="${urlReports}" class="btn btn-outline-primary btn-sm"><i class="fa-solid fa-magnifying-glass-plus"></i> View reports</a>
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