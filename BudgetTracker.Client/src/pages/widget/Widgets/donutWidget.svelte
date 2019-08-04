<script lang="ts">
	import { DonutWidgetViewModel } from './../../../generated-types';
	import { formatMoney } from './../../../services/Shared';
	import { ChartConfiguration } from 'c3';
	import tabler from './../../../tabler';
	import { onMount } from 'svelte';
	import c3 from 'c3';

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

	let chartDiv: HTMLElement;

	let formatPercentage = function(value: number) { return Math.floor(value * 10000) / 100 + '%'; }

	let height = fullScreen ? 700 : 300;

	let refresh = function() {
		if (!chartDiv) {
			return () => {};
		}

		if (!model.names) {
			model.names = [];
		}

		if (!model.values) {
			model.values = [];
		}

		let chartNames : any = {};
		for(var i =0; i < model.values.length; i++) {
			chartNames['data' + i] = model.names[i];
		}
		let chartGroups = model.values.map((_,i)=>'data' + i);

		let chartItems = model.values.map((v,i) => ['data' + i, v ]);
		let sum = model.values.reduce((a,b) => a+b, 0);
		
		var params: ChartConfiguration = {
			bindto: chartDiv,
			data: {
				columns: chartItems,
				type: "donut",
				groups: [ chartGroups ],
				colors: {
					'data1': tabler.colors["blue"]
				},
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
				show: false,
				inset: {
					anchor: 'top-left',
					x: 5,
					y: 0,
					step: 999
				}
			},
			donut: {
				title: formatMoney(sum),
				label: {
					format: function (_: number, ratio: number, id: string) {
						var trimmed = chartNames[id].length > 16 ? chartNames[id].substring(0, 15) + "…" : chartNames[id];
						return trimmed + " (" + formatPercentage(ratio) + ")";
					}
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

		var chart = c3.generate(params)

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
	<div bind:this="{chartDiv}" class="pie-chart"></div>
</div>