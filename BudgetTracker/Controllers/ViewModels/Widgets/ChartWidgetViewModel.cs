using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class ChartWidgetViewModel : WidgetViewModel
    {
        public ChartWidgetViewModel(string providerName, string accountName,
            ChartKind kind,
            ObjectRepository repository,
            TableViewModel vm) : base(null, null)
        {
            ChartKind = kind;
            Title = accountName;
            LoadData(repository, vm, providerName, accountName);
        }

        public ChartWidgetViewModel(WidgetModel model, ObjectRepository repository, int? period, TableViewModel vm) :
            base(model, new ChartWidgetSettings(model.Properties.ToDictionary(v => v.Key, v => v.Value)))
        {
            var chartWidgetSettings = (ChartWidgetSettings) Settings;

            ChartKind = chartWidgetSettings.ChartKind;
            Title = Title ?? chartWidgetSettings.AccountName;
            Period = period;

            LoadData(repository, vm, chartWidgetSettings.ProviderName, chartWidgetSettings.AccountName);
        }

        public int? Period { get; set; }

        public ChartKind ChartKind { get; set; }

        private void LoadData(ObjectRepository repository, TableViewModel vm, string providerName, string accountName)
        {
            var column = repository.Set<MoneyColumnMetadataModel>().First(v =>
                v.Provider == providerName &&
                (v.AccountName == accountName ||
                 v.UserFriendlyName == accountName));

            var columnsToChart = new List<MoneyColumnMetadataModel> {column};

            if (column.IsComputed && column.ChartList.Any())
            {
                columnsToChart = repository.Set<MoneyColumnMetadataModel>().Where(v =>
                    column.ChartList.Contains(v.Provider + "/" + v.AccountName) ||
                    column.ChartList.Contains(v.UserFriendlyName)).ToList();

                if (ChartKind == ChartKind.Linear)
                {
                    columnsToChart.Add(column);
                }
            }

            var chartItems = new List<ChartItem>();

            foreach (var row in vm.Values.Where(v => IsApplicable(v.When, Period)))
            {
                foreach (var header in columnsToChart)
                {
                    var value = row.Cells.FirstOrDefault(v => v.Column == header);
                    if (value?.AdjustedValue == null)
                        continue;

                    chartItems.Add(new ChartItem
                    {
                        When = row.When,
                        Name = (header.IsComputed ? "" : (header.Provider + "/")) +
                               (header.UserFriendlyName ?? header.AccountName),
                        Value = value.AdjustedValue.Value,
                        Ccy = value.Ccy
                    });
                }
            }

            var names = chartItems.Select(v => v.Name).Distinct().ToList();
            Dates = chartItems.Select(v => v.When).Distinct().ToList();

            Values = names
                .SelectMany(name => Dates.Select(date => (name, date)))
                .Select(s => (s.name, s.date, chartItems.FirstOrDefault(v => v.Name == s.name && v.When == s.date)))
                .GroupBy(v => v.name)
                .ToDictionary(v => v.Key, v => v.Select(s => s.Item3).Where(s => s != null).ToList());
        }

        public List<DateTime> Dates { get; set; }
        public Dictionary<string, List<ChartItem>> Values { get; set; }

        public override string TemplateName => WidgetExtensions.AsPath("~/Views/Widget/Widgets/ChartWidget.cshtml");
        public override int Columns => 4;
        public override int Rows => 2;
    }

    public class ChartItem
    {
        public DateTime When { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public string Ccy { get; set; }
    }
}