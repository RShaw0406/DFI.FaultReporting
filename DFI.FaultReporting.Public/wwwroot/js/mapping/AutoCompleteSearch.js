// FUNCTION SUMMARY:
// This method is used for setting up the autocomplete search box
function initSearch() {
    // Find search box on page
    var searchBox = document.getElementById('searchBox');

    // Add event for when user begins to input text
    searchBox.addEventListener('input', search);
}

// Initialise the search
initSearch();


function search() {
    var query = document.getElementById('searchBox').value;

    if (query == "") {
        map.setCamera({
            // Center map on Northern Ireland
            center: [-6.8, 54.65],
            // Set zoom to 3 to show as much of Northern Ireland as possible
            zoom: 3,
            // Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland
            maxBounds: mapBounds,
        });
    }
    else {
        searchURL.searchFuzzy(atlas.service.Aborter.timeout(10000), query, {
            lat: 54.65,
            lon: -6.8,
            btmRight: '53.9,-8.3',
            topLeft: '55.4,-5.3',
            radius: 100000,
            view: 'Auto',
            language: 'en-GB'
        }).then(results => {
            console.log(results);
            if (results.results.length > 0) {
                autocomplete(document.getElementById("searchBox"), results);
            }
            else {
                map.setCamera({
                    // Center map on Northern Ireland
                    center: [-6.8, 54.65],
                    // Set zoom to 3 to show as much of Northern Ireland as possible
                    zoom: 3,
                    // Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland
                    maxBounds: mapBounds,
                });
            }
        });
    }
}

function autocomplete(inp, results) {
    var geoData = results.geojson.getFeatures();

    console.log(geoData);
    /*the autocomplete function takes two arguments,
    the text field element and an array of possible autocompleted values:*/
    var currentFocus;
    var a, b, i, val = inp.value;
    /*close any already open lists of autocompleted values*/
    closeAllLists();
    if (!val) { return false; }
    currentFocus = -1;
    /*create a DIV element that will contain the items (values):*/
    a = document.createElement("DIV");
    a.setAttribute("id", inp.id + "autocomplete-list");
    a.setAttribute("class", "autocomplete-items");
    /*append the DIV element as a child of the autocomplete container:*/
    inp.parentNode.appendChild(a);
    /*for each item in the array...*/
    for (i = 0; i < geoData.features.length; i++) {
        if (geoData.features[i].properties.address.countrySubdivisionName == "Northern Ireland") {
            /*create a DIV element for each matching element:*/
            b = document.createElement("DIV");
            /*make the matching letters bold:*/
            b.innerHTML = "<strong>" + geoData.features[i].properties.address.freeformAddress.substr(0, val.length) + "</strong>";
            b.innerHTML += geoData.features[i].properties.address.freeformAddress.substr(val.length);
            /*insert a input field that will hold the current array item's value:*/
            b.innerHTML += "<input type='hidden' value='" + geoData.features[i].properties.address.freeformAddress + "'>";

            //var geoData = results[i].geojson.getFeatures();

            console.log(geoData.features[i].bbox);

            b.innerHTML += "<input type='hidden' value='" + geoData.features[i].bbox + "'>";
            /*execute a function when someone clicks on the item value (DIV element):*/
            b.addEventListener("click", function (e) {
                /*insert the value for the autocomplete text field:*/
                inp.value = this.getElementsByTagName("input")[0].value;
                /*close the list of autocompleted values,
                (or any other open lists of autocompleted values:*/
                //Set the camera to the bounds of the results.

                var boundingBox = this.getElementsByTagName("input")[1].value

                console.log(boundingBox);

                const boundingBoxArray = boundingBox.split(",");

                map.setCamera({
                    bounds: boundingBoxArray,
                    // Add boundaries to restrict how user can pan and zoom the map control - needed to stop user panning or zooming to a location outside of Northern Ireland
                    maxBounds: mapBounds,
                });
                closeAllLists();
            });
            a.appendChild(b);
        }
    }
    /*execute a function presses a key on the keyboard:*/
    inp.addEventListener("keydown", function (e) {
        var x = document.getElementById(inp.id + "autocomplete-list");
        if (x) x = x.getElementsByTagName("div");
        if (e.keyCode == 40) {
            /*If the arrow DOWN key is pressed,
            increase the currentFocus variable:*/
            currentFocus++;
            /*and and make the current item more visible:*/
            addActive(x);
        } else if (e.keyCode == 38) { //up
            /*If the arrow UP key is pressed,
            decrease the currentFocus variable:*/
            currentFocus--;
            /*and and make the current item more visible:*/
            addActive(x);
        } else if (e.keyCode == 13) {
            /*If the ENTER key is pressed, prevent the form from being submitted,*/
            e.preventDefault();
            if (currentFocus > -1) {
                /*and simulate a click on the "active" item:*/
                if (x) x[currentFocus].click();
            }
        }
    });
    function addActive(x) {
        /*a function to classify an item as "active":*/
        if (!x) return false;
        /*start by removing the "active" class on all items:*/
        removeActive(x);
        if (currentFocus >= x.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = (x.length - 1);
        /*add class "autocomplete-active":*/
        x[currentFocus].classList.add("autocomplete-active");
    }
    function removeActive(x) {
        /*a function to remove the "active" class from all autocomplete items:*/
        for (var i = 0; i < x.length; i++) {
            x[i].classList.remove("autocomplete-active");
        }
    }
    function closeAllLists(elmnt) {
        /*close all autocomplete lists in the document,
        except the one passed as an argument:*/
        var x = document.getElementsByClassName("autocomplete-items");
        for (var i = 0; i < x.length; i++) {
            if (elmnt != x[i] && elmnt != inp) {
                x[i].parentNode.removeChild(x[i]);
            }
        }
    }

    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}