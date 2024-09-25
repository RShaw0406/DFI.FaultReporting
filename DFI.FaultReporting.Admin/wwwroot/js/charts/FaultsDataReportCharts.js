//CODE SUMMARY:
//This file contains the JavaScript code to generate the charts for the Faults Data Report page.


//FUNCTION SUMMARY:
//This function is called when the page is loaded. It calls the functions to generate the charts.
function initCharts() {

    //Generate the charts.
    GenerateStatusBarChart();
    GeneratePriorityBarChart();
    GenerateTypeBarChart();
}

//FUNCTION SUMMARY:
//This function generates the status bar chart.
function GenerateStatusBarChart() {

    //Get the control for the status bar chart.
    var statusBarChartControl = document.getElementById('statusBarChart');

    //Create an array of the fault status descriptions.
    const faultStatusDescriptions = [];

    //Loop over each of the fault statuses in the session and add to array.
    faultStatuses.forEach(function (faultStatus) {
        faultStatusDescriptions.push(faultStatus.faultStatusDescription);
    });

    //Create a new chart object for the status bar chart
    var statusBarChart = new Chart(statusBarChartControl, {
        type: 'bar',
        data: {
            labels: faultStatusDescriptions
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
                        text: 'Number of faults'
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
                        text: 'Fault status'
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

    var faultTypeLoopCounter = 0;

    //Loop over each of the fault types in the session.
    faultTypes.forEach(function (faultType) {

        faultTypeLoopCounter++;

        var faultTypeDescription = faultType.faultTypeDescription; 

        //Create an array of length equal to the number of fault statuses.
        var faultCounts = [];
        faultCounts.length = faultStatuses.length;

        //Loop over each of the fault statuses in the session.
        for (let i = 0; i < faultStatuses.length; i++) {
            faultCounts[i] = 0;
        }

        //Loop over each of the faults in the session.
        faults.forEach(function (fault) {


            //Fault is related to the current fault type.
            if (fault.faultTypeID == faultType.id) {

                //Loop over each of the fault statuses in the session.
                for (let i = 0; i < faultStatuses.length; i++) {

                    //Fault is related to the current fault status.
                    if (fault.faultStatusID == faultStatuses[i].id) {

                        faultCounts[i] = faultCounts[i] + 1;
                    }
                }
            }
        });

        //Create a new data set for the chart.
        var dataSet = {
            label: faultTypeDescription,
            data: faultCounts,
            backgroundColor: chartColors[faultTypeLoopCounter],
            borderColor: chartColors[faultTypeLoopCounter],
            borderWidth: 1,
            stack: "Stack 0"
        };

        statusBarChart.data.datasets.push(dataSet);

        statusBarChart.update();
    });
}

//FUNCTION SUMMARY:
//This function generates the priority bar chart.
function GeneratePriorityBarChart() {

    //Get the control for the priority bar chart.
    var priorityBarChartControl = document.getElementById('priorityBarChart');

    //Create an array of the fault priority
    const priorityRatings = [];

    //Loop over each of the fault priorities in the session and add to array.
    faultPriorities.forEach(function (faultPriority) {
        priorityRatings.push(faultPriority.faultPriorityRating);
    });

    //Create a new chart object for the priority bar chart.
    var priorityBarChart = new Chart(priorityBarChartControl, {
        type: 'bar',
        data: {
            labels: priorityRatings
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
                        text: 'Number of faults'
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
                        text: 'Priority rating'
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

    var faultTypeLoopCounter = 0;

    //Loop over each of the fault types in the session.
    faultTypes.forEach(function (faultType) {

        faultTypeLoopCounter++;

        var faultTypeDescription = faultType.faultTypeDescription;

        //Create an array of length equal to the number of fault priorities.
        var faultCounts = [];
        faultCounts.length = faultPriorities.length;

        //Loop over each of the fault priorities in the session.
        for (let i = 0; i < faultPriorities.length; i++) {
            faultCounts[i] = 0;
        }

        //Loop over each of the faults in the session.
        faults.forEach(function (fault) {


            //Fault is related to the current fault type.
            if (fault.faultTypeID == faultType.id) {

                //Loop over each of the fault priorities in the session.
                for (let i = 0; i < faultPriorities.length; i++) {

                    //Fault is related to the current fault priority.
                    if (fault.faultPriorityID == faultPriorities[i].id) {

                        faultCounts[i] = faultCounts[i] + 1;
                    }
                }
            }
        });

        //Create a new data set for the chart.
        var dataSet = {
            label: faultTypeDescription,
            data: faultCounts,
            backgroundColor: chartColors[faultTypeLoopCounter],
            borderColor: chartColors[faultTypeLoopCounter],
            borderWidth: 1,
            stack: "Stack 0"
        };

        priorityBarChart.data.datasets.push(dataSet);

        priorityBarChart.update();
    });
}

//FUNCTION SUMMARY:
//This function generates the type bar chart.
function GenerateTypeBarChart() {

    //Get the control for the type bar chart.
    var typeBarChartControl = document.getElementById('typeBarChart');

    //Create an array of the fault type descriptions.
    const typeDescriptions = [];

    //Loop over each of the fault types in the session and add to array.
    faultTypes.forEach(function (faultType) {
        typeDescriptions.push(faultType.faultTypeDescription);
    });

    //Create a new chart object for the type bar chart.
    var typeBarChartChart = new Chart(typeBarChartControl, {
        type: 'bar',
        data: {
            labels: typeDescriptions
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
            skipNull: true,
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    grid: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: 'Number of faults'
                    },
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                },
                x: {
                    grid: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: 'Fault type'
                    }
                }
            },
            plugins: {
                legend: {
                    display: true
                }
            }
        }
    });

    var faultTypeLoopCounter = 0;

    //Loop over each of the fault types in the session.
    for (let i = 0; i < faultTypes.length; i++) {       

        faultTypeLoopCounter++;

        //Get the fault type description.
        var faultTypeDescription = faultTypes[i].faultTypeDescription;

        //Create an array of length equal to the number of fault types.
        var faultCounts = [];
        faultCounts.length = faultTypes.length;

        //Loop over each of the fault types in the session.
        for (let i = 0; i < faultTypes.length; i++) {
            faultCounts[i] = null;
        }

        //Loop over each of the faults in the session and set the associated count to 0.
        faultCounts[i] = 0;

        //Loop over each of the faults in the session.
        faults.forEach(function (fault) {
            //Fault is related to the current fault type.
            if (fault.faultTypeID == faultTypes[i].id) {
                //Increment the count for the current fault type.
                faultCounts[i] = faultCounts[i] + 1;
            }
        });    

        //Create a new data set for the chart.
        var dataSet = {
            label: faultTypeDescription,
            data: faultCounts,
            backgroundColor: chartColors[faultTypeLoopCounter],
            borderColor: chartColors[faultTypeLoopCounter],
            borderWidth: 1
        };

        typeBarChartChart.data.datasets.push(dataSet);

        typeBarChartChart.update();
    }
}