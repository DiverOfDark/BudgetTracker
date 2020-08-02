<script lang="ts">
	import * as interfaces from './../../../generated-types';
	import { formatMoney } from './../../../services/Shared';

	import tabler from './../../../tabler';
	import { onMount, onDestroy } from 'svelte';
	
	import Chart from 'chart.js';

	export let model: interfaces.LinearChartWidgetViewModel = {
        title: '',
        period: 0,
        values: [],
        columns: 0,
        id: '',
        kind: '',
        rows: 0,
		settings: {},
		dates: [],
		exemptTransfers: false
	};
	
	export let fullScreen = false;

	let currentChart: Chart;

	let chartCanvas: HTMLCanvasElement;

	let formatPercentage = function(value: number) { return Math.floor(value * 10000) / 100 + '%'; }

	let height = fullScreen ? 700 : 308;
	let showAxis = fullScreen;

	let refresh = function() {
		if (!chartCanvas) {
			return;
		}

		let datasets = model.values.map((x,i) => { return {
						label: x.label,
						data: x.values.reverse().map(s=>Number.isFinite(s) ? s : NaN),
						pointRadius: 0,
						borderColor: tabler.getNiceColor(i),
						backgroundColor: tabler.getNiceColor(i)
					}
				});

		if (currentChart) {
			currentChart.destroy()
		}

		currentChart = new Chart(chartCanvas, {
			type: 'line',
			data: {
				labels: model.dates.reverse(),
				datasets: datasets
			}, 
			options: {
				maintainAspectRatio: false,
				legend: {
					display: true
				},
				elements: {
            		line: { tension: 0 }
				},
				plugins: {
					datalabels: {
						display: false,
					},
				},
				responsive: true,
				scales: {
					xAxes: [{
						display: false,
						scaleLabel: {
							display: false
						}
					}],
					yAxes: [{
						display: showAxis,
						stacked: true,
						scaleLabel: {
							display: showAxis
						}
					}]
				},
				hover: {
					mode: 'index'
				},
				tooltips: {
					mode: 'index',
					intersect: false,
					callbacks: {
						label: (tooltipItem: any, data: any) => {
							var title = data.datasets[tooltipItem.datasetIndex].label;
							var value = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
							
							var sum = data.datasets.map((s: any)=>s.data[tooltipItem.index]).filter(Number.isFinite).reduce((a: number,b: number)=>a+b, 0)

							var ratio = sum == 0 ? 0 : value / sum
							
							var label = formatMoney(value) + " (" + formatPercentage(ratio) + ")";

							return title+": " + label;
						}
					}
				}
			}
		})
	}

	$: { model && refresh(); }
	onMount(() => refresh());
	onDestroy(() => currentChart && currentChart.destroy());

	// used implicitly
	height;
</script>

<div class="card-body">
	<div class="h4 text-muted-dark text-center">
		{model.title}
	</div>
</div>
<div class="card-chart-bg" style="height: {height}px">
	<canvas bind:this="{chartCanvas}"></canvas>
</div>