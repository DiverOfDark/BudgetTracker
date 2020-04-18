<script lang="ts">
	import * as interfaces from './../../../generated-types';
    import { formatMoney } from './../../../services/Shared';
	import tabler from './../../../tabler';
	import { onMount, onDestroy } from 'svelte';

	import Chart from 'chart.js';

	//@ts-ignore
	import ChartDataLabels from 'chartjs-plugin-datalabels';
	//@ts-ignore
	import ChartDoughnutLabel from 'chartjs-plugin-doughnutlabel';

	export let model: interfaces.ExpensesWidgetViewModel = {
        title: '',
        period: 0,
        names: [],
        values: [],
        columns: 0,
        expenseSettings: {
            currency: ''
        },
        id: '',
        kind: '',
        rows: 0,
        settings: {}
    };

    let currentChart: Chart;
	let chartCanvas: HTMLCanvasElement;

    let refresh = function() {
        if (!chartCanvas) {
            return;
        }

        if (currentChart) {
            currentChart.destroy();
        }

        currentChart = new Chart(chartCanvas, {
            type: 'doughnut',
            plugins: [ChartDataLabels, ChartDoughnutLabel],
            data: {
                datasets: [{
                    data: model.values || [],
                    backgroundColor: tabler.getNiceColors()
                }],
                labels: model.names || []
            },
            options: {
                maintainAspectRatio: false,
                legend: {
                    display: true,
                    position: "right"
                },
                plugins: {
                    doughnutlabel: {
                        labels: [
                        {
                            text: formatMoney((model.values || []).reduce((a,b)=>a+b,0)),
                            font: {
                                size: '16'
                            }
                        }]
                    },
                    datalabels: {
                        color: 'white',
                        font: {
                            weight: 'bold'
                        },
                        formatter: (value: any, _: any) => {
                            if (value < model.values.reduce((a,b) => a+b, 0) * 0.05) {
                                return "";
                            }
                            return formatMoney(value)
                        }
                    }
                },
                tooltips: {
                    callbacks: {
                        label: (tooltipItem: any, data: any) => {
                            var title = data.labels[tooltipItem.index] || '';
                            return title;
                        },
                        footer: (tooltipItems: any, data: any) => {
                            var tooltipItem = tooltipItems[0];
                            var value = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
                            var label = formatMoney(value) + " " + model.expenseSettings.currency;

                            return label;
                        }
                    }
                }
            }
        });
    }

    $: { model && refresh(); }
    onMount(() => refresh());
   	onDestroy(() => currentChart && currentChart.destroy());
</script>

<div class="card-body">
    <div class="h4 text-muted-dark text-center">
        {model.title} ({model.period} месяц)
    </div>
    <div style="height: 300px">
        <canvas bind:this="{chartCanvas}"></canvas>
    </div>
</div>