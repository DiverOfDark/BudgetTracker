<script lang="ts">
	import * as interfaces from './../../../generated-types';
	import Link from '../../../svero/Link.svelte';
    import { formatMoney, formatDate, formatDateJs } from './../../../services/Shared';
	import { onMount, onDestroy } from 'svelte';
	import {compare} from './../../../services/Shared'
	import tabler from './../../../tabler';
	import Chart from 'chart.js';

	export let model: interfaces.LastValueWidgetViewModel = {
		account: '',
		color: '',
		colorYear: '',
		currentDate: '',
		currentValue: 0,
		currentValueFormatted: '',
		delta: '',
		deltaYear: '',
		description: '',
		exemptTransfers: false,
		graphKind: interfaces.GraphKindEnum.Differential,
		incompleteData: false,
		isCompact: false,
		provider: '',
		id: '',
		values: [],
		columns: 0,
		kind: '',
		rows: 0,
		settings: {},
		title: ''
	};

	let currentChart: Chart;
	let chartCanvas: HTMLCanvasElement;

	let colorSuffix = "";
	$: { colorSuffix = model.incompleteData ? "bg-gray-dark-darkest" : ""; }

	let hexColor = "#000000";

	let trend = "fe-trending-up";
	
	$: {
		switch (model.colorYear) {
			case "red":
				hexColor = "#e74c3c";
				trend = "fe-trending-down";
				break;
			case "green":
				hexColor = "#5eba00";
				break;
			case "yellow":
				hexColor = "#f1c40f";
				trend = "fe-activity";
				break;
			case "blue":
				hexColor = "#467fcf";
				trend = "fe-trending-down";
				break;
		}
	}

	let refresh = function() {
		if (!model.isCompact && chartCanvas) {
			var goodItems = Object.entries(model.values).sort((a,b)=>compare(a[0], b[0])).filter(v=>v[1]);
			var chartItems : number[] = goodItems.map(v => <number> v[1]);
			var datesItems : string[] = goodItems.map(v => <string> v[0]);

			if (model.graphKind.getId() == interfaces.GraphKindEnum.Differential.getId())
			{
				chartItems = chartItems.map((a,b) => b == 0 ? a : a - chartItems[b-1]);
				datesItems.slice(1);
			}
			else if (model.graphKind.getId() == interfaces.GraphKindEnum.CumulativeFromTimePeriod.getId())
			{
				chartItems = chartItems.map(a => a - chartItems[0]);
			}

			var yMin = chartItems.reduce((a,b) => a < b ? a : b, 0);
			var yMax = chartItems.reduce((a,b) => a > b ? a : b, 0);

			var diff = (yMax - yMin) * 0.1;
			yMin -= diff;
			yMax += diff;

			var values = chartItems;
			var dates = datesItems.map(formatDateJs);

			if (currentChart) {
				currentChart.destroy();
			}

			currentChart = new Chart(chartCanvas, {
				type: 'line',
				data: {
					labels: dates,
					datasets: [{
						label: model.title,
						data: values,
						pointRadius: 0,
						borderColor: tabler.getNiceColor(0),
						backgroundColor: tabler.getNiceColor(0)
					}]
				}, 
				options: {
					maintainAspectRatio: false,
					legend: {
						display: false
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
							display: false,
							stacked: true,
							scaleLabel: {
								display: false
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
								var label = formatMoney(value);

								return title+": " + label;
							}
						}
					}
				}
			})
		}
	}
	
	$: { model && refresh(); }

	onMount(() => refresh());
	onDestroy(() => currentChart && currentChart.destroy());

	hexColor; trend; colorSuffix; formatDate; formatMoney; Link;
</script>

{#if model.isCompact}
	<div class="card-body p-3 text-center {colorSuffix}" style="height:100%">
		<div class="h1 mt-5">
			{model.currentValueFormatted}
		</div>
        <div class="text-muted">
            {model.title}
			<Link href="/Chart/{encodeURIComponent(model.provider)}/{encodeURIComponent(model.account)}/{encodeURIComponent(model.exemptTransfers)}">
		        <span class="fe {trend}"></span>
	        </Link>
            <br/>
            <small>
				{formatDate(model.currentDate)}
            </small>
        </div>
    </div>
{:else}
	<div class="card-status bg-{model.colorYear}"></div>
    <div class="card-body {colorSuffix}">
        <div class="float-right" alt="{model.description}" title="{model.description}" data-toggle="tooltip">
	        <div class="card-value text-{model.colorYear}">
		        {model.deltaYear}
	        </div>
	        <div class="h6 text-right text-{model.color}">
		        {model.delta} <sub>/d</sub>
	        </div>
        </div>
        <h3 class="mb-1 text-nowrap">
			{model.currentValueFormatted}
        </h3>
        <div class="text-muted text-nowrap">
            {model.title}
	        <Link href="/Chart/{encodeURIComponent(model.provider)}/{encodeURIComponent(model.account)}">
		        <span class="fe {trend}"></span>
	        </Link>
        </div>
        <h4 class="m-0">
            <small class="text-muted">
				{formatDate(model.currentDate)}	
            </small>
        </h4>
    </div>
	<div class="card-chart-bg {colorSuffix}">
		<canvas bind:this="{chartCanvas}"></canvas>
	</div>
{/if}
