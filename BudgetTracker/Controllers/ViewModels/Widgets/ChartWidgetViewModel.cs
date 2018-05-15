using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class ChartWidgetViewModel : WidgetViewModel
    {
        public ChartWidgetViewModel(WidgetModel model, ObjectRepository repository, TableViewModel vm) : base(model, new ChartWidgetSettings(model.Properties.ToDictionary(v=>v.Key, v=>v.Value)))
        {
            ChartWidgetSettings = (ChartWidgetSettings) Settings;

            vm = new TableViewModel(vm)
            {
                ShowAll = true,
                ShowControls = false,
                ShowDelta = false
            };

            var column = repository.Set<MoneyColumnMetadataModel>().First(v =>
                v.Provider == ChartWidgetSettings.ProviderName &&
                (v.AccountName == ChartWidgetSettings.AccountName ||
                 v.UserFriendlyName == ChartWidgetSettings.AccountName));

            var columnsToChart = new List<MoneyColumnMetadataModel>{column};
            
            if (column.IsComputed)
            {
                columnsToChart = repository.Set<MoneyColumnMetadataModel>().Where(v =>
                    column.ChartList.Contains(v.Provider + "/" + v.AccountName) ||
                    column.ChartList.Contains(v.UserFriendlyName)).ToList();
            }

            var chartItems = new List<ChartItem>();
            
            foreach (var row in vm.Values)
            {
                foreach (var header in columnsToChart)
                {
                    var value = row.Cells.FirstOrDefault(v => v.Column == header);
                    if (value?.Value == null)
                        continue;
    
                    chartItems.Add(new ChartItem
                    {
                        When = row.When,
                        Name = header.UserFriendlyName ?? header.AccountName,
                        Value = value.Value.Value,
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

        public ChartWidgetSettings ChartWidgetSettings { get; }

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