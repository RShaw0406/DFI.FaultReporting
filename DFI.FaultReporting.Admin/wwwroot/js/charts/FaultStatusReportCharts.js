function initCharts() {
    GenerateBarChart();
}

function GenerateLineChart() {
    var lineChartControl = document.getElementById('lineChart');
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
                    display: false
                }
            }
        }
    });

    //Loop over each of the fault types in the session.
    faultTypes.forEach(function (faultType) {

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
            backgroundColor: 'rgba(255, 99, 132, 0.2)',
            borderColor: 'rgba(255, 99, 132, 1)',
            borderWidth: 1
        };

        barChart.data.datasets.push(dataSet);

        barChart.update();



    });

    var data = barChart.data.datasets;
}