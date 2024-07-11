function initCharts() {

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