function initCharts() {

    GenerateStatusBarChart();

    GenerateTypeBarChart();
}

function GenerateStatusBarChart() {
    var statusBarChartControl = document.getElementById('statusBarChart');

    const claimStatusesDescriptions = [];

    claimStatuses.forEach(function (claimStatus) {
        claimStatusesDescriptions.push(claimStatus.claimStatusDescription);
    });

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

function GenerateTypeBarChart() {
    var typeBarChartControl = document.getElementById('typeBarChart');


    const typeDescriptions = [];

    claimTypes.forEach(function (claimType) {
        typeDescriptions.push(claimType.claimTypeDescription);
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

    for (let i = 0; i < claimTypes.length; i++) {

        claimTypeLoopCounter++;

        var claimTypeDescription = claimTypes[i].claimTypeDescription;

        var claimCounts = [];
        claimCounts.length = claimTypes.length;

        for (let i = 0; i < claimTypes.length; i++) {
            claimCounts[i] = null;
        }

        claimCounts[i] = 0;

        claims.forEach(function (claim) {
            if (claim.claimTypeID == claimTypes[i].id) {
                claimCounts[i] = claimCounts[i] + 1;
            }
        });

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