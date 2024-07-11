function initCharts() {

    GenerateStatusBarChart();

    GeneratePriorityBarChart();

    GenerateTypeBarChart();
}

function GenerateStatusBarChart() {
    var statusBarChartControl = document.getElementById('statusBarChart');

    const faultStatusDescriptions = [];

    faultStatuses.forEach(function (faultStatus) {
        faultStatusDescriptions.push(faultStatus.faultStatusDescription);
    });

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

function GeneratePriorityBarChart() {
    var priorityBarChartControl = document.getElementById('priorityBarChart');

    const priorityRatings = [];

    faultPriorities.forEach(function (faultPriority) {
        priorityRatings.push(faultPriority.faultPriorityRating);
    });

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

function GenerateTypeBarChart() {
    var typeBarChartControl = document.getElementById('typeBarChart');


    const typeDescriptions = [];

    faultTypes.forEach(function (faultType) {
        typeDescriptions.push(faultType.faultTypeDescription);
    });

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

    for (let i = 0; i < faultTypes.length; i++) {       

        faultTypeLoopCounter++;

        var faultTypeDescription = faultTypes[i].faultTypeDescription;

        var faultCounts = [];
        faultCounts.length = faultTypes.length;

        for (let i = 0; i < faultTypes.length; i++) {
            faultCounts[i] = null;
        }

        faultCounts[i] = 0;

        faults.forEach(function (fault) {
            if (fault.faultTypeID == faultTypes[i].id) {
                faultCounts[i] = faultCounts[i] + 1;
            }
        });    

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