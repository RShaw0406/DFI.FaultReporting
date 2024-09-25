//CODE SUMMARY:
//This file contains the JavaScript code to generate the charts for the Repairs Data Report page.

//FUNCTION SUMMARY:
//This function is called when the page is loaded. It calls the functions to generate the charts.
function initCharts() {

    //Initialise the search functionality.
    initSearch();

    //Generate the charts.
    GenerateStatusBarChart();
    GenerateTargetMetPieChart();
}

//FUNCTION SUMMARY:
//This function generates the status bar chart.
function GenerateStatusBarChart() {

    //Get the control for the status bar chart.
    var statusBarChartControl = document.getElementById('statusBarChart');

    //Create an array of the repair status descriptions.
    const repairStatusDescriptions = [];

    //Loop over each of the repair statuses in the session and add to array.
    repairStatuses.forEach(function (repairStatus) {
        repairStatusDescriptions.push(repairStatus.repairStatusDescription);
    });

    //Create a new chart object for the status bar chart.
    var statusBarChart = new Chart(statusBarChartControl, {
        type: 'bar',
        data: {
            labels: repairStatusDescriptions
        },
        options: {
            animation: {
                onComplete: () => {
                    delayed = true;
                },
                delay: (context) => {
                    let delay = 0;
                    if (context.type === 'data' && context.mode === 'default' && !delayed) {
                        delay = context.dataIndex * 300 + context.datasetIndex * 100;
                    }
                    return delay;
                },
            },
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    grid: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: 'Number of repairs'
                    },
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    },
                    stacked: true
                },
                x: {
                    grid: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: 'Repair status'
                    },
                    stacked: true
                }
            },
            plugins: {
                legend: {
                    display: true
                }
            }
        }
    });

    var repairStatusLoopCounter = 0;

    //Loop over each of the repair statuses in the session.
    for (let i = 0; i < repairStatuses.length; i++) {

        repairStatusLoopCounter++;

        //Get the repair status description.
        var repairStatusDescription = repairStatuses[i].repairStatusDescription;

        //Create an array to store the number of repairs for each status.
        var repairCounts = [];
        repairCounts.length = repairStatuses.length;

        for (let i = 0; i < repairStatuses.length; i++) {
            repairCounts[i] = null;
        }

        repairCounts[i] = 0;

        //Loop over each of the repairs in the session.
        repairs.forEach(function (repair) {
            if (repair.repairStatusID == repairStatuses[i].id) {
                repairCounts[i]++;
            }
        });

        //Create a new data set for the status bar chart.
        var dataSet = {
            label: repairStatusDescription,
            data: repairCounts,
            backgroundColor: chartColors[repairStatusLoopCounter],
            borderColor: chartColors[repairStatusLoopCounter],
            borderWidth: 1
        };

        statusBarChart.data.datasets.push(dataSet);

        statusBarChart.update();
    }
}

//FUNCTION SUMMARY:
//This function generates the target met pie chart.
function GenerateTargetMetPieChart() {
    //Get the control for the target met pie chart.
    var targetMetChartControl = document.getElementById('targetMetPieChart');

    //Create an array of the target met descriptions.
    const targetMetDescriptions = ['Target met', 'Target not met'];

    //Create a new chart object for the target met pie chart.
    var targetMetPieChart = new Chart(targetMetChartControl, {
        type: 'pie',
        data: {
            labels: targetMetDescriptions
        },
        options: {
            animation: {
                onComplete: () => {
                    delayed = true;
                },
                delay: (context) => {
                    let delay = 0;
                    if (context.type === 'data' && context.mode === 'default' && !delayed) {
                        delay = context.dataIndex * 300 + context.datasetIndex * 100;
                    }
                    return delay;
                },
            },
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true
                }
            }
        }
    });

    const repairCounts = [];

    var totalTargetMet = 0;

    var totalTargetNotMet = 0;

    //Loop over each of the repairs in the session.
    repairs.forEach(function (repair) {
        //Check if the actual repair date is less than or equal to the target repair date.
        if (repair.actualRepairDate <= repair.repairTargetDate) {
            //Increment the total target met count.
            totalTargetMet++;
        }
        else
        {
            //Increment the total target not met count.
            totalTargetNotMet++;
        }
    });

    //Add the total target met and total target not met counts to the repair counts array.
    repairCounts.push(totalTargetMet);
    repairCounts.push(totalTargetNotMet);

    //Create a new data set for the target met pie chart.
    var dataSet = {
        data: repairCounts,
        backgroundColor: ['#ccffcc', '#ff8080'],
    };

    targetMetPieChart.data.datasets.push(dataSet);

    targetMetPieChart.update();
}

//FUNCTION SUMMARY:
//This function initialises the search functionality to enable users to search for a contractor.
function initSearch() {
    //Find search box on page.
    var searchBox = document.getElementById('searchBox');

    if (searchBox != null) {
        //Add event for when user begins to input text.
        searchBox.addEventListener('input', search);
    }
}

//FUNCTION SUMMARY:
//This function is used to search for a contractor based on the user input in the search box.
//This function is executed when the user begins to type in the search box.
function search() {

    //Get the user input from the search box.
    var query = document.getElementById('searchBox').value;

    //If the search box is empty, submit the form.
    if (!query) {
        document.getElementById("filterForm").submit();
    }
    else {
        //Search for contractors based on the user input.
        autoCompleteSearch(document.getElementById("searchBox"), contractors);
    }
}


//FUNCTION SUMMARY:
//This function is used to create an autocomplete list of contractors based on the user input.
function autoCompleteSearch(searchControl, contractors) {
    //Declare variable to store the index of the item the user is currently focused on.
    var focusItem;

    //Add input event to detct when user begins to type in search box.
    searchControl.addEventListener("input", function (e) {
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
        this.parentNode.appendChild(outerDiv);

        //Loop through the contractors.
        contractors.forEach(function (contractor) {

            //If the contractor name starts with the user input.
            if (contractor.contractorName.substr(0, searchValue.length).toUpperCase() == searchValue.toUpperCase()) {

                //Create inner div element to display each search result.
                innerDiv = document.createElement("DIV");
                //Make the characters in the search result bold that match the user input.
                innerDiv.innerHTML = "<strong>" + contractor.contractorName.substr(0, searchValue.length) + "</strong>";
                innerDiv.innerHTML += contractor.contractorName.substr(searchValue.length);
                //Add hidden input field to store the contractor name.
                innerDiv.innerHTML += "<input type='hidden' value='" + contractor.contractorName + "'>";

                //Add click event to set the search box value to the selected contractor name and submit the form.
                innerDiv.addEventListener("click", function (e) {

                    //Set the search box value to the selected contractor name.
                    searchControl.value = this.getElementsByTagName("input")[0].value;

                    //Close any previous search results.
                    closePrevSearchResults();

                    document.getElementById("searchID").value = contractor.id;

                    //Submit the form.
                    document.getElementById("filterForm").submit();
                });

                //Append the inner div to the outer div.
                outerDiv.appendChild(innerDiv);
            }
        });
    });

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