function initCharts() {

    GenerateBarChart();

    GenerateRadarChart();
}

function GenerateRadarChart() {
    var radarChartControl = document.getElementById('radarChart');

    const faultStatusDescriptions = [];

    faultStatuses.forEach(function (faultStatus) {
        faultStatusDescriptions.push(faultStatus.faultStatusDescription);
    });

    var radarChart = new Chart(radarChartControl, {
        type: 'radar',
        data: {
            labels: faultStatusDescriptions
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scale: {
                ticks: {
                    stepSize: 1
                }
            },
            plugins: {
                legend: {
                    display: true
                },
                filler: {
                    propagate: false
                },
                'samples-filler-analyser': {
                    target: 'chart-analyser'
                }
            },
            interaction: {
                intersect: false
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
            fill: true
        };

        radarChart.data.datasets.push(dataSet);

        radarChart.update();
    });

    var data = radarChart.data.datasets;
}

function GenerateBarChart() {
    var barChartControl = document.getElementById('barChart');

    const faultStatusDescriptions = [];

    faultStatuses.forEach(function (faultStatus) {
        faultStatusDescriptions.push(faultStatus.faultStatusDescription);
    });

    var barChart = new Chart(barChartControl, {
        type: 'bar',
        data: {
            labels: faultStatusDescriptions
        },
        options: {
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

        barChart.data.datasets.push(dataSet);

        barChart.update();
    });
}