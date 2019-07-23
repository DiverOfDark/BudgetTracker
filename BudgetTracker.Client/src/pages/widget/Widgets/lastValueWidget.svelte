<script lang="ts">
	import { LastValueWidgetViewModel } from './../../../generated-types';
    import { formatMoney, formatDate } from './../../../services/Shared';

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

	let chartDiv;

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

	if (!model.isCompact) {
/*
	var goodItems = Model.Values.OrderByDescending(v => v.Key).Where(v => v.Value.HasValue).ToList();
	var chartItems = goodItems.Select(v=>v.Value.Value).ToList();
	var datesItems = goodItems.Select(v => v.Key).ToList();

	if (Model.GraphKind == GraphKind.Differential)
	{
		chartItems = chartItems.Skip(1).Zip(chartItems, (x, y) => y - x).ToList();
		datesItems = datesItems.Skip(1).ToList();
	}
	else if (Model.GraphKind == GraphKind.CumulativeFromTimePeriod)
	{
		var fod = chartItems.LastOrDefault();
		chartItems = chartItems.Select(v => v - fod).ToList();
	}
	
	var yMin = chartItems.Min();
	var yMax = chartItems.Max();
	var diff = (yMax - yMin) * 0.1;
	yMin -= diff;
	yMax += diff;
	
	var values = string.Join(", ", chartItems.Select(v => Math.Round(v, 4).ToString(CultureInfo.InvariantCulture)));
	var dates = string.Join(", ", datesItems.Select(v => $"'{v:yyyy-MM-dd}'"));


		(function() {

			var columns = [
				['data1', @Html.Raw(values)],
				['x', @Html.Raw(dates)]
			];
			
			var chart = c3.generate({
				bindto: '#chart-@chartId',
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
						'data1': '@Model.Title'
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
						value: function (value) {
							return d3.format(',.2f')(value);
						}
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
						max: @yMax.ToString(CultureInfo.InvariantCulture),
						min: @yMin.ToString(CultureInfo.InvariantCulture)
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
					pattern: ['@hexColor']
				}
			});
			
			window.onfocus = function() {
				chart.load({
					columns: columns
				});
			};
		})();
*/
	}

	hexColor; trend; colorSuffix; formatDate; formatMoney; chartDiv;
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
