TODO: last value widget
<!--
@using System.Globalization
@using Controllers.ViewModels.Widgets
@model LastValueWidgetViewModel

@{
	var chartId = Guid.NewGuid().ToString("N");
	var colorSuffix = Model.IncompleteData ? "bg-gray-dark-darkest" : ""; 

	var hexColor = "#000000";

	var trend = "fe-trending-up";
	
	switch (Model.ColorYear)
	{
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

@if (Model.IsCompact)
{
	<div class="card-body p-3 text-center @colorSuffix" style="height:100%">
		<div class="h1 mt-5">
			@Model.FormatValue(Model.CurrentValue)
		</div>
        <div class="text-muted">
            @Model.Title
	        <a href="@Url.Action("Chart", "Table", new {provider = Model.Provider, account = Model.Account, exemptTransfers = Model.ExemptTransfers })">
		        <span class="fe @trend"></span>
	        </a>
	        <a href="@Url.Action("Burst", "Table", new {provider = Model.Provider, account = Model.Account })">
		        <span class="fe fe-zoom-in"></span>
	        </a>
            <br/>
            <small>
                @Model.FormatDate(Model.CurrentDate.ToLocalTime().Date)
            </small>
        </div>
    </div>
}
else
{
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

	<div class="card-status bg-@Model.ColorYear"></div>
    <div class="card-body @colorSuffix">
        <div class="float-right" alt="@Model.Description" title="@Model.Description" data-toggle="tooltip">
	        <div class="card-value text-@Model.ColorYear">
		        @Model.DeltaYear
	        </div>
	        <div class="h6 text-right text-@Model.Color">
		        @Model.Delta <sub>/d</sub>
	        </div>
        </div>
        <h3 class="mb-1 text-nowrap">
            @Model.FormatValue(Model.CurrentValue)
        </h3>
        <div class="text-muted text-nowrap">
            @Model.Title
	        <a href="@Url.Action("Chart", "Table", new {provider = Model.Provider, account = Model.Account })">
		        <span class="fe @trend"></span>
	        </a>
	        <a href="@Url.Action("Burst", "Table", new {provider = Model.Provider, account = Model.Account })">
		        <span class="fe fe-zoom-in"></span>
	        </a>
        </div>
        <h4 class="m-0">
            <small class="text-muted">
                @Model.FormatDate(Model.CurrentDate.ToLocalTime().Date)
            </small>
        </h4>
    </div>
	<div class="card-chart-bg @colorSuffix">
		<div id="chart-@chartId" style="height: 100%"></div>
	</div>
	<script>
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
	</script>
}
-->