using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.JsModel;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class DonutWidgetViewModel : WidgetViewModel
    {
        public DonutWidgetViewModel(WidgetModel model, int? period, TableViewModel vm) :
            base(model, new DonutWidgetSettings(model.Properties.ToDictionary(v => v.Key, v => v.Value)))
        {
            var chartWidgetSettings = (DonutWidgetSettings) Settings;

            Title = Title ?? chartWidgetSettings.AccountName;
            Period = period;

            LoadData(vm, chartWidgetSettings.ProviderName, chartWidgetSettings.AccountName);
        }

        public int? Period { get; set; }

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
            }

            Names = new List<string>();
            Values = new List<double>();

            var newTitle = Title;

            while (vm.Values.Count > 0 && columnsToChart.Any(s => !vm.Values[0].CalculatedCells[s].IsOk))
            {
                vm.Values.RemoveAt(0);

                newTitle = Title + " - " + vm.Values[0].When.ToString("dd.MM.yyyy");
            }

            Title = newTitle;
            
            foreach (var header in columnsToChart)
            {
                var cell = vm.Values.OrderByDescending(v => v.When).First(v => IsApplicable(v.When, Period));

                var item = cell.CalculatedCells.GetValueOrDefault(header);

                var value = item?.Value;
                if (value == null || double.IsNaN(value.Value))
                    continue;

                var name = (header.IsComputed ? "" : (header.Provider + "/")) +
                           (header.UserFriendlyName ?? header.AccountName);

                var realValue = value.Value;
                Names.Add(name);
                Values.Add(realValue);
            }
        }

        public List<string> Names { get; set; }
        public List<double> Values { get; set; }

        public override int Columns => 4;
        public override int Rows => 2;
    }
}