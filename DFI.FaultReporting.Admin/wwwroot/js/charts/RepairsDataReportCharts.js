function initCharts() {

    initSearch();

    GenerateStatusBarChart();

    GenerateTargetMetPieChart();
}

function GenerateStatusBarChart() {

    var statusBarChartControl = document.getElementById('statusBarChart');

    const repairStatusDescriptions = [];

    repairStatuses.forEach(function (repairStatus) {
        repairStatusDescriptions.push(repairStatus.repairStatusDescription);
    });

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

    for (let i = 0; i < repairStatuses.length; i++) {

        repairStatusLoopCounter++;

        var repairStatusDescription = repairStatuses[i].repairStatusDescription;

        var repairCounts = [];
        repairCounts.length = repairStatuses.length;

        for (let i = 0; i < repairStatuses.length; i++) {
            repairCounts[i] = null;
        }

        repairCounts[i] = 0;

        repairs.forEach(function (repair) {
            if (repair.repairStatusID == repairStatuses[i].id) {
                repairCounts[i]++;
            }
        });

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

function GenerateTargetMetPieChart() {
    var targetMetChartControl = document.getElementById('targetMetPieChart');

    const targetMetDescriptions = ['Target met', 'Target not met'];

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

    repairs.forEach(function (repair) {
        if (repair.actualRepairDate <= repair.repairTargetDate) {
            totalTargetMet++;
        }
        else
        {
            totalTargetNotMet++;
        }
    });

    repairCounts.push(totalTargetMet);
    repairCounts.push(totalTargetNotMet);

    var dataSet = {
        data: repairCounts,
        backgroundColor: ['#ccffcc', '#ff8080'],
    };

    targetMetPieChart.data.datasets.push(dataSet);

    targetMetPieChart.update();
}

function initSearch() {
    // Find search box on page
    var searchBox = document.getElementById('searchBox');

    if (searchBox != null) {
        // Add event for when user begins to input text
        searchBox.addEventListener('input', search);
    }
}


function search() {
    var query = document.getElementById('searchBox').value;

    if (!query) {
        document.getElementById("filterForm").submit();
    }
    else {
        autocomplete(document.getElementById("searchBox"), contractors);
    }
}

function autocomplete(inp, contractors) {
    /*the autocomplete function takes two arguments,
    the text field element and an array of possible autocompleted values:*/
    var currentFocus;
    /*execute a function when someone writes in the text field:*/
    inp.addEventListener("input", function (e) {
        var a, b, i, val = this.value;
        /*close any already open lists of autocompleted values*/
        closeAllLists();
        if (!val) { return false; }
        currentFocus = -1;
        /*create a DIV element that will contain the items (values):*/
        a = document.createElement("DIV");
        a.setAttribute("id", this.id + "autocomplete-list");
        a.setAttribute("class", "autocomplete-items");
        /*append the DIV element as a child of the autocomplete container:*/
        this.parentNode.appendChild(a);
        /*for each item in the array...*/
        contractors.forEach(function (contractor) {
            if (contractor.contractorName.substr(0, val.length).toUpperCase() == val.toUpperCase()) {

                /*create a DIV element for each matching element:*/
                b = document.createElement("DIV");

                /*make the matching letters bold:*/
                b.innerHTML = "<strong>" + contractor.contractorName.substr(0, val.length) + "</strong>";
                b.innerHTML += contractor.contractorName.substr(val.length);
                /*insert a input field that will hold the current array item's value:*/
                b.innerHTML += "<input type='hidden' value='" + contractor.contractorName + "'>";


                /*execute a function when someone clicks on the item value (DIV element):*/
                b.addEventListener("click", function (e) {
                    /*insert the value for the autocomplete text field:*/
                    inp.value = this.getElementsByTagName("input")[0].value;
                    /*close the list of autocompleted values,
                    (or any other open lists of autocompleted values:*/
                    closeAllLists();

                    document.getElementById("searchID").value = contractor.id;

                    document.getElementById("filterForm").submit();
                });
                a.appendChild(b);
            }
        });
    });
    /*execute a function presses a key on the keyboard:*/
    inp.addEventListener("keydown", function (e) {
        var x = document.getElementById(this.id + "autocomplete-list");
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
    /*execute a function when someone clicks in the document:*/
    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}