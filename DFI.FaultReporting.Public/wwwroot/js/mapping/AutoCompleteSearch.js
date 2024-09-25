//CODE SUMMARY:
//This file sets up and controls the autocomplete search used on map controls. 
//A user can begin to type in a location and the search will return a list of possible locations that match the input. 
//The user can then select a location from the list and the map will zoom to that location.


// FUNCTION SUMMARY:
// This method is used for setting up the autocomplete search box.
function initSearch() {

    var searchBox = document.getElementById('searchBox');

    if (searchBox != null) {
        //Add input event to detect when user begins to type in search box.
        searchBox.addEventListener('input', search);
    }
}

//Initialise the search
initSearch();


//FUNCTION SUMMARY:
//This function is used to search for locations based on the user input in the search box.
//This function is executed when the user begins to type in the search box.
function search() {

    //Get the user input from the search box.
    var query = document.getElementById('searchBox').value;

    if (query == "") {
        map.setCamera({
            //Center map on Northern Ireland.
            center: [-6.8, 54.65],
            //Set zoom to 3 to show as much of Northern Ireland as possible.
            zoom: 3,
            //Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland.
            maxBounds: mapBounds,
        });
    }
    else {
        //Search for locations based on the user input using Azure Maps Search Services.
        searchURL.searchFuzzy(atlas.service.Aborter.timeout(10000), query, {
            lat: 54.65,
            lon: -6.8,
            btmRight: '53.9,-8.3',
            topLeft: '55.4,-5.3',
            radius: 100000,
            view: 'Auto',
            language: 'en-GB'
        }).then(results => {
            //If results are returned, display the results in the autocomplete list.
            if (results.results.length > 0) {
                autoCompleteSearch(document.getElementById("searchBox"), results);
            }
            //If no results are returned, set the camera to the default location of Northern Ireland.
            else {
                map.setCamera({
                    //Center map on Northern Ireland.
                    center: [-6.8, 54.65],
                    //Set zoom to 3 to show as much of Northern Ireland as possible.
                    zoom: 3,
                    //Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland.
                    maxBounds: mapBounds,
                });
            }
        });
    }
}

//FUNCTION SUMMARY:
//This function is used to create the autocomplete list based on the search results.
function autoCompleteSearch(searchControl, results) {

    //Get the search result geojson data.
    var geoData = results.geojson.getFeatures();

    //Declare variable to store the index of the item the user is currently focused on.
    var focusItem;

    //Declare variables to store the search and create elements to display the search results.
    var outerDiv, innerDiv, i, searchValue = searchControl.value;

    //Close any previous search results.
    closePrevSearchResults();

    //If the search box is empty, return false.
    if (!searchValue) { return false; }
    focusItem = -1;

    //Create outer div element to contain the search results.
    outerDiv = document.createElement("DIV");
    outerDiv.setAttribute("id", searchControl.id + "autocomplete-list");
    outerDiv.setAttribute("class", "autocomplete-items");

    //Append the outer div to the parent node of the search control.
    searchControl.parentNode.appendChild(outerDiv);

    //Loop through the search results.
    for (i = 0; i < geoData.features.length; i++) {

        //Only display search results for Northern Ireland.
        if (geoData.features[i].properties.address.countrySubdivisionName == "Northern Ireland") {

            //Create inner div element to display each search result.
            innerDiv = document.createElement("DIV");
            //Make the characters in the search result bold that match the user input.
            innerDiv.innerHTML = "<strong>" + geoData.features[i].properties.address.freeformAddress.substr(0, searchValue.length) + "</strong>";
            innerDiv.innerHTML += geoData.features[i].properties.address.freeformAddress.substr(searchValue.length);
            //Add hidden input fields to store the address and bounding box of the search result.
            innerDiv.innerHTML += "<input type='hidden' value='" + geoData.features[i].properties.address.freeformAddress + "'>";
            innerDiv.innerHTML += "<input type='hidden' value='" + geoData.features[i].bbox + "'>";

            //When the user clicks on a search result, set the search box value to the selected location and zoom the map to that location.
            innerDiv.addEventListener("click", function (e) {
                //Set the search box value to the selected location.
                searchControl.value = this.getElementsByTagName("input")[0].value;

                //Get the bounding box of the selected location.
                var boundingBox = this.getElementsByTagName("input")[1].value;

                //Split the bounding box string into an array.
                const boundingBoxArray = boundingBox.split(",");

                //Zoom the map to the selected location.
                map.setCamera({
                    bounds: boundingBoxArray,
                    //Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland.
                    maxBounds: mapBounds,
                });

                //Close the search results.
                closePrevSearchResults();
            });

            //Append the inner div to the outer div.
            outerDiv.appendChild(innerDiv);
        }
    }

    //Add event listeners to detect when the user presses the arrow keys or enter key to navigate the search results.
    searchControl.addEventListener("keydown", function (e) {

        //Get the search results list control.
        var autocompleteList = document.getElementById(searchControl.id + "autocomplete-list");

        if (autocompleteList) autocompleteList = autocompleteList.getElementsByTagName("div");

        //If the arrow DOWN key is pressed.
        if (e.keyCode == 40) {
            //Increment the focusItem index.
            focusItem++;
            makeActive(autocompleteList);
        }
        //If the arrow UP key is pressed.
        else if (e.keyCode == 38) {
            //Decrement the focusItem index.
            focusItem--;
            makeActive(autocompleteList);
        }
        //If the ENTER key is pressed.
        else if (e.keyCode == 13) {
            //Prevent the form from being submitted.
            e.preventDefault();

            //If the focusItem index is greater than -1.
            if (focusItem > -1) {
                //Execute the click event of the selected search result.
                if (autocompleteList) autocompleteList[focusItem].click();
            }
        }
    });

    //FUNCTION SUMMARY:
    //This function is used to classify an item as "active" in the search results.
    function makeActive(searchItem) {

        //If no search results are returned, return false.
        if (!searchItem) return false;

        //Call the makeInactive function to remove the "active" class from all search results.
        makeInactive(searchItem);

        //If the focusItem index is greater than the number of search results, set the focusItem index to 0.
        if (focusItem >= searchItem.length) focusItem = 0;

        //If the focusItem index is less than 0, set the focusItem index to the number of search results.
        if (focusItem < 0) focusItem = (searchItem.length - 1);

        //Add the "active" class to the search result the user is currently focused.
        searchItem[focusItem].classList.add("autocomplete-active");
    }

    //FUNCTION SUMMARY:
    //This function is used to remove the "active" class from all search results.
    function makeInactive(searchItem) {

        //Loop through the search results and remove the "active" class.
        for (var i = 0; i < searchItem.length; i++) {
            searchItem[i].classList.remove("autocomplete-active");
        }
    }

    //FUNCTION SUMMARY:
    //This function is used to close any previous search results.
    function closePrevSearchResults(outerDivElement) {

        //Get the search results list control.
        var autocompleteList = document.getElementsByClassName("autocomplete-items");

        //Loop through the search results and remove the search results.
        for (var i = 0; i < autocompleteList.length; i++) {
            if (outerDivElement != autocompleteList[i] && outerDivElement != searchControl) {
                autocompleteList[i].parentNode.removeChild(autocompleteList[i]);
            }
        }
    }

    //Add event listener to detect when the user clicks outside of the search results to close the search results.
    document.addEventListener("click", function (e) {
        closePrevSearchResults(e.target);
    });
}