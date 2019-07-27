Expenses!!
<!--
@using System.Globalization
@using Controllers.ViewModels.Payment
@using Controllers.ViewModels.Widgets
@model ExpensesWidgetViewModel

@{
    var columnsData = values.Select((v, i) => $"['col{i}', {v.ToString(CultureInfo.InvariantCulture)}]");
    var namesData = names.Select((v, i) => $"'col{i}': '{v.Replace("\'","\\'")}'");
}

<div class="card-body">
    <div class="h4 text-muted-dark text-center">
        @Model.Title (@Model.Period месяц)
    </div>
    <div id="chart-@id" class="pie-chart" style="height: 100%;"></div>
</div>

<script>
    $(document).ready(function() {

        var columns = [
            @Html.Raw(string.Join(",", columnsData))
        ];
        
        var chart = c3.generate({
            bindto: '#chart-@id',
            data: {
                columns: columns,
                type: 'donut',
                order: null,
                names: {
                    @Html.Raw(string.Join(",", namesData))
                }
            },
            donut: {
                title: d3.format(',.2f')(@values.Sum()) + " @Model.ExpenseSettings.Currency",
                label: {
                    format: function (value) {
                        return d3.format(',.2f')(value);
                    }
                }
            },
            tooltip: {
                format: {
                    value: function (value) {
                        return d3.format(',.2f')(value) + " @Model.ExpenseSettings.Currency";
                    }
                }
            },
            size: {
                height: 300
            },
            axis: {
            },
            legend: {
                show: true,
                position: 'right'
            },
            padding: {
                bottom: 0,
                top: 0
            }
        });
        
        window.onfocus = function() {
            chart.load({
                columns: columns
            });
        };
    });
</script>
-->