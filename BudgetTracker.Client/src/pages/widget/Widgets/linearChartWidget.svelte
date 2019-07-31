<script lang="ts">
	import { LinearChartWidgetViewModel } from './../../../generated-types';
	import { formatMoney } from './../../../services/Shared';
	import { ChartConfiguration } from 'c3';
	import tabler from './../../../tabler';
	import { onMount } from 'svelte';
	import c3 from 'c3';

	export let model: LinearChartWidgetViewModel = {
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

	let chartDiv: HTMLElement;

	let formatPercentage = function(value: number) { return Math.floor(value * 10000) / 100 + '%'; }

	let height = fullScreen ? 700 : 300;
	let showAxis = fullScreen;

	let refresh = function() {
		if (!chartDiv) {
			return () => {};
		}

		let chartNames : any = {};
		for(var i =0; i < model.values.length; i++) {
			chartNames['data' + i] = model.values[i].label;
		}
		
		let chartGroups = model.values.map((_,i)=>'data' + i);

		var prefilter = (a:any) : number => a !== "NaN" ? a : NaN;

		var chartItems = model.values.map((v,i) => ['data' + i, ...v.values.map(t=>prefilter(t))]);

		var params: ChartConfiguration = {
			bindto: chartDiv,
			data: {
				columns: [
					...chartItems,
					['x', ...model.dates]
				],
				type: "area", // default type of chart
				groups: [ chartGroups ],
				colors: {
					'data1': tabler.colors["blue"]
				},
				x: 'x',
				names: chartNames
			},
			size: {
				height: height
			},
			padding: {
				bottom: -10
			},
			legend: {
				position: 'inset',
				padding: 0,
				show: true,
				inset: {
					anchor: 'top-left',
					x: 5,
					y: 0,
					step: 999
				}
			},
			point: {
				show: true
			},
			axis: {
				y: {
					show: showAxis
				},
				x: {
					type: 'timeseries',
					show: showAxis
				}
			},
			tooltip: {
					format: {
						value: function (value: number, ratio: number) {
							return formatMoney(value) + " (" + formatPercentage(ratio) + ")";
						}
					}
			}
		};

		let chart = c3.generate(params)

		return chart.unload;
	}

	$: { model && refresh(); }
	onMount(() => refresh());
</script>

<div class="card-body">
	<div class="h4 text-muted-dark text-center">
		{model.title}
	</div>
</div>
<div class="card-chart-bg" style="height: 100%; max-height: {height}px">
	<div bind:this="{chartDiv}"></div>
</div>