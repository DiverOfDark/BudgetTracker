using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.JsModel;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class LinearChartWidgetViewModel : WidgetViewModel
    {
        public LinearChartWidgetViewModel(string providerName, string accountName,
            TableViewModel vm, bool exemptTransfers) : base(null, null)
        {
            ExemptTransfers = exemptTransfers;
            Title = accountName;
            LoadData(vm, providerName, accountName);
        }

        public LinearChartWidgetViewModel(WidgetModel model, int? period, TableViewModel vm) :
            base(model, new DonutWidgetSettings(model.Properties.ToDictionary(v => v.Key, v => v.Value)))
        {
            var chartWidgetSettings = (DonutWidgetSettings) Settings;

            Title = Title ?? chartWidgetSettings.AccountName;
            Period = period;

            LoadData(vm, chartWidgetSettings.ProviderName, chartWidgetSettings.AccountName);
        }

        public int? Period { get; set; }

        public bool ExemptTransfers { get; }

        private void LoadData(TableViewModel vm, string providerName, string accountName)
        {
            var column = vm.Headers.First(v =>
                v.Provider == providerName &&
                (v.AccountName == accountName ||
                 v.UserFriendlyName == accountName));

            var columnsToChart = new List<MoneyColumnMetadataJsModel> {column};

            if (column.IsComputed && column.ChartList.Any())
            {
                columnsToChart = vm.Headers.Where(v =>
                    column.ChartList.Contains(v.Provider + "/" + v.AccountName) ||
                    column.ChartList.Contains(v.UserFriendlyName)).ToList();

                columnsToChart.Add(column);
            }

            var values = columnsToChart.ToDictionary(v => v, header => new ChartValue
            {
                Label = (header.IsComputed ? "" : (header.Provider + "/")) +
                        (header.UserFriendlyName ?? header.AccountName),
                Values = new List<double>()
            });

            Dates = new List<string>();
            Values = values.Values.ToList(); 

            foreach (var row in vm.Values.Where(v => IsApplicable(v.When, Period)))
            {
                var when = row.When.Date;

                Dates.Add(when.ToString("yyyy-MM-dd"));
                
                foreach (var header in columnsToChart)
                {
                    var item = row.CalculatedCells.GetValueOrDefault(header);

                    var chartItem = values[header];

                    var value = ExemptTransfers ? item?.AdjustedValue : item?.Value;
                    chartItem.Values.Add(value ?? double.NaN);
                }
            }
        }

        public List<string> Dates { get; set; }
        public List<ChartValue> Values { get; set; }

        public override int Columns => 4;
        public override int Rows => 2;
    }
}