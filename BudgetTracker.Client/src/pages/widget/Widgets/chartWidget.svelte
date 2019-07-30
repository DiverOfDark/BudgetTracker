<script lang="ts">
	import { ChartWidgetViewModel } from './../../../generated-types';
	import { formatMoney, formatDate } from './../../../services/Shared';
	import { ChartConfiguration } from 'c3';
	import tabler from './../../../tabler';
	import { onMount } from 'svelte';
	import c3 from 'c3';
	import {compare} from './../../../services/Shared'

	export let model: ChartWidgetViewModel = {
        title: '',
        period: 0,
        values: [],
        columns: 0,
        id: '',
        kind: '',
        rows: 0,
		settings: {},
		chartKind: '',
		dates: [],
		exemptTransfers: false
	};
	
	export let fullScreen = false;

	let chartDiv: HTMLElement;

	let formatPercentage = function(value: number) { return Math.floor(value * 100) / 100 + '%'; }

	let chartClass = '';
	let height = fullScreen ? 700 : 300;
	let showAxis = fullScreen;

	let refresh = function() {
		let chartNames : any = {};
		for(var i =0; i < model.values.length; i++) {
			chartNames['data' + i] = model.values[i];
		}
		let chartGroups = model.values.map((_,i)=>'data' + i);
		let sum = 0;
		let type = "";
		let showLegend = true;

		var names = chartNames;

		var chartItems: any[] = [];
		var dates: any[] = [];

		if (model.chartKind == "donut") {
			type = "donut";
			chartClass = "pie-chart";
			showLegend = false;
			chartItems = model.values.map((v,i) => ['data' + i, v.values.sort((a,b)=>compare(a.value,b.value))[0] ]);
			sum = model.values.map(s=>s.values.sort((a,b)=>compare(a.value,b.value))[0]).map(s=>s.value).reduce((a,b) => a+b);
			dates = model.dates.sort((a,b)=>compare(a,b)).map(x=>[formatDate(x)])[0];
		}
		if (model.chartKind == 'linear') {
			type = "area";
			chartItems = model.values.map((v,i) => ['data' + i, ...v.values.map(t=>formatMoney(t.value))]);
			dates = model.dates.map(formatDate);
		}
			
		var columns = [
			...chartItems,
			['x', ...dates]
		];

		var params: ChartConfiguration;

		params = {
			bindto: chartDiv,
			data: {
				columns: columns,
				type: type, // default type of chart
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
				show: showLegend,
				inset: {
					anchor: 'top-left',
					x: 5,
					y: 0,
					step: 999
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

		if (model.chartKind == 'linear')
		{
			params.point = {
                                show: true
							};
			params.axis = {
		                        y: {
			                        show: showAxis
		                        },
		                        x: {
			                        type: 'timeseries',
			                        show: showAxis
		                        }
	                        };
		} else {
			params.donut = {
				                title: formatMoney(sum),
				                label: {
					               format: function (_: number, ratio: number, id: string) {
										var trimmed = names[id].length > 16 ? names[id].substring(0, 15) + "…" : names[id];
						                return trimmed + " (" + formatPercentage(ratio) + ")";
					               }
				                }
			                }
		}

		console.log(params);

		var chart = c3.generate(params)

		return chart.unload;
	}

	$: { model && refresh(); }
	onMount(() => refresh());

	chartClass;
</script>

<div class="card-body">
	<div class="h4 text-muted-dark text-center">
		{model.title}
	</div>
</div>
<div class="card-chart-bg" style="height: 100%">
	<div bind:this="{chartDiv}" class="{chartClass}"></div>
</div>