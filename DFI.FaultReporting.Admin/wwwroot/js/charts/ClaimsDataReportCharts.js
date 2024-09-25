//CODE SUMMARY:
//This file contains the JavaScript code to generate the charts for the Claims Data Report page.


//FUNCTION SUMMARY:
//This function is called when the page is loaded. It calls the functions to generate the charts.
function initCharts() {

    //Generate the charts.
    GenerateStatusBarChart();
    GenerateTypeBarChart();
}

//FUNCTION SUMMARY:
//This function generates the status bar chart.
function GenerateStatusBarChart() {

    //Get the control for the status bar chart.
    var statusBarChartControl = document.getElementById('statusBarChart');

    //Create an array of the claim status descriptions.
    const claimStatusesDescriptions = [];

    //Loop over each of the claim statuses in the session and add to array.
    claimStatuses.forEach(function (claimStatus) {
        claimStatusesDescriptions.push(claimStatus.claimStatusDescription);
    });

    //Create a new chart object for the status bar chart
    var statusBarChart = new Chart(statusBarChartControl, {
        type: 'bar',
        data: {
            labels: claimStatusesDescriptions
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
                        text: 'Number of claims'
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
                        text: 'Claim status'
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

    //Loop counter for the claim types.
    var claimTypeLoopCounter = 0;

    //Loop over each of the claim types in the session.
    claimTypes.forEach(function (claimType) {

        claimTypeLoopCounter++;

        var claimTypeDescription = claimType.claimTypeDescription;

        //Create an array of length equal to the number of claim statuses.
        var claimCounts = [];
        claimCounts.length = claimStatuses.length;

        //Loop over each of the claim statuses in the session.
        for (let i = 0; i < claimStatuses.length; i++) {
            claimCounts[i] = 0;
        }

        //Loop over each of the claims in the session.
        claims.forEach(function (claim) {


            //Claim is related to the current claim type.
            if (claim.claimTypeID == claimType.id) {

                //Loop over each of the claim statuses in the session.
                for (let i = 0; i < claimStatuses.length; i++) {

                    //Claim is related to the current claim status.
                    if (claim.claimStatusID == claimStatuses[i].id) {

                        claimCounts[i] = claimCounts[i] + 1;
                    }
                }
            }
        });

        //Create a new dataset for the claim type.
        var dataSet = {
            label: claimTypeDescription,
            data: claimCounts,
            backgroundColor: chartColors[claimTypeLoopCounter],
            borderColor: chartColors[claimTypeLoopCounter],
            borderWidth: 1,
            stack: "Stack 0"
        };

        statusBarChart.data.datasets.push(dataSet);

        statusBarChart.update();
    });
}

//FUNCTION SUMMARY:
//This function generates the type bar chart.
function GenerateTypeBarChart() {

    //Get the control for the type bar chart.
    var typeBarChartControl = document.getElementById('typeBarChart');

    //Create an array of the claim type descriptions.
    const typeDescriptions = [];

    //Loop over each of the claim types in the session and add to array.
    claimTypes.forEach(function (claimType) {
        typeDescriptions.push(claimType.claimTypeDescription);
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
                        text: 'Number of claims'
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
                        text: 'Claim type'
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

    var claimTypeLoopCounter = 0;

    //Loop over each of the claim types in the session.
    for (let i = 0; i < claimTypes.length; i++) {

        claimTypeLoopCounter++;

        //Get the claim type description.
        var claimTypeDescription = claimTypes[i].claimTypeDescription;

        //Create an array of length equal to the number of claim types.
        var claimCounts = [];
        claimCounts.length = claimTypes.length;

        //Loop over each of the claim types in the session and set the asscoiated count to null.
        for (let i = 0; i < claimTypes.length; i++) {
            claimCounts[i] = null;
        }

        //Set the count for the current claim type to 0.
        claimCounts[i] = 0;

        //Loop over each of the claims in the session.
        claims.forEach(function (claim) {

            //Claim is related to the current claim type.
            if (claim.claimTypeID == claimTypes[i].id) {

                //Increment the count for the current claim type.
                claimCounts[i] = claimCounts[i] + 1;
            }
        });

        //Create a new dataset for the claim type.
        var dataSet = {
            label: claimTypeDescription,
            data: claimCounts,
            backgroundColor: chartColors[claimTypeLoopCounter],
            borderColor: chartColors[claimTypeLoopCounter],
            borderWidth: 1
        };

        typeBarChartChart.data.datasets.push(dataSet);

        typeBarChartChart.update();
    }
}