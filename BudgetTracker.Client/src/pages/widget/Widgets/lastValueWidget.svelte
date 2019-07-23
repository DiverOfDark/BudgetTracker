<script lang="ts">
	import { LastValueWidgetViewModel } from './../../../generated-types';
    import { formatMoney, formatDate, formatDateJs } from './../../../services/Shared';
	import { onMount } from 'svelte';
	import {compare} from './../../../services/Shared'
	import c3 from 'c3';

	export let model: LastValueWidgetViewModel = {
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
		graphKind: '',
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

	let chartDiv: HTMLElement;

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
	onMount(() => {
		if (!model.isCompact) {
			var goodItems = Object.entries(model.values).sort((a,b)=>compare(a[0], b[0])).filter(v=>v[1]);
			var chartItems = goodItems.map(v => v[1]);
			var datesItems = goodItems.map(v => v[0]);

			if (model.graphKind == 'Differential')
			{
				chartItems = chartItems.map((a,b) => b == 0 ? a : a - chartItems[b-1]);
				datesItems.slice(1);
			}
			else if (model.graphKind == 'CumulativeFromTimePeriod')
			{
				chartItems = chartItems.map(a => a - chartItems[0]);
			}

			var yMin = chartItems.reduce((a,b) => a < b ? a : b);
			var yMax = chartItems.reduce((a,b) => a > b ? a : b);

			var diff = (yMax - yMin) * 0.1;
			yMin -= diff;
			yMax += diff;

			var values = chartItems;
			var dates = datesItems.map(formatDateJs);

			var columns = [
						['data1', ...values],
						['x', ...dates]
					];

			var chart = c3.generate({
				bindto: chartDiv,
				padding: {
					bottom: -10,
					left: -1,
					right: -1
				},
				size: {
					height: 64
				},
				data: {
					x: 'x',
					names: {
						'data1': model.title
					},
					columns: columns,
					type: 'area-spline'
				},
				legend: {
					show: false
				},
				transition: {
					duration: 0
				},
				point: {
					show: true
				},
				tooltip: {
					show: true,
					format: {
						value: formatMoney
					}
				},
				axis: {
					y: {
						padding: {
							bottom: 0
						},
						show: false,
						tick: {
							outer: true
						},
						max: yMax,
						min: yMin
					},
					x: {
						type: 'timeseries',
						padding: {
							left: 0,
							right: 0
						},
						show: false
					}
				},
				color: {
					pattern: [hexColor]
				}
			});
			chart.flush();

			return chart.unload;
		}
		return ()=>{};
	});

	hexColor; trend; colorSuffix; formatDate; formatMoney;
</script>

{#if model.isCompact}
	<div class="card-body p-3 text-center {colorSuffix}" style="height:100%">
		<div class="h1 mt-5">
			{model.currentValueFormatted}
		</div>
        <div class="text-muted">
            {model.title}
	        <a href="/Table/Chart?provider={model.provider}&account={model.account}&exemptTransfers={model.exemptTransfers}">
		        <span class="fe {trend}"></span>
	        </a>
	        <a href="/Table/Burst?provider={model.provider}&account={model.account}">
		        <span class="fe fe-zoom-in"></span>
	        </a>
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
	        <a href="/Table/Chart?provider={model.provider}&account={model.account}">
		        <span class="fe @trend"></span>
	        </a>
	        <a href="/Table/Burst?provider={model.provider}&account={model.account}">
		        <span class="fe fe-zoom-in"></span>
	        </a>
        </div>
        <h4 class="m-0">
            <small class="text-muted">
				{formatDate(model.currentDate)}	
            </small>
        </h4>
    </div>
	<div class="card-chart-bg {colorSuffix}">
		<div bind:this="{chartDiv}" style="height: 100%"></div>
	</div>
{/if}
