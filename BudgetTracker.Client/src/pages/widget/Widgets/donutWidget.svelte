<script lang="ts">
	import { DonutWidgetViewModel } from './../../../generated-types';
	import { formatMoney } from './../../../services/Shared';

	import tabler from './../../../tabler';
	import { onMount, onDestroy } from 'svelte';
	import Chart from 'chart.js';

	//@ts-ignore
	import ChartDataLabels from 'chartjs-plugin-datalabels';
	//@ts-ignore
	import ChartDoughnutLabel from 'chartjs-plugin-doughnutlabel';

	export let model: DonutWidgetViewModel = {
        title: '',
		period: 0,
		names: [],
        values: [],
        columns: 0,
        id: '',
        kind: '',
        rows: 0,
		settings: {}
	};
	
	export let fullScreen = false;

	let currentChart: Chart;

	let canvas: HTMLCanvasElement;

	let formatPercentage = function(value: number) { return Math.floor(value * 10000) / 100 + '%'; }

	let height = fullScreen ? 700 : 300;

	let refresh = function() {
		if (!canvas) {
			return
		}

		if (currentChart) {
			currentChart.destroy();
		}

		currentChart = new Chart(canvas, {
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
					display: false
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
						formatter: (value: any, context: any) => {
							var ratio = value / context.dataset.data.reduce((a: number,b: number)=>a+b, 0)
							if (ratio > 0.04) {
								return formatPercentage(ratio)
							}
							return "";
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
							var ratio = value / data.datasets[tooltipItem.datasetIndex].data.reduce((a: number,b: number)=>a+b, 0)
							var label = formatMoney(value) + " (" + formatPercentage(ratio) + ")";

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

	// used implicitly
	height;
</script>

<div class="card-body">
	<div class="h4 text-muted-dark text-center">
		{model.title}
	</div>
</div>
<div class="card-chart-bg" style="height: 100%; max-height: {height}px">
	<canvas bind:this="{canvas}"></canvas>
</div>