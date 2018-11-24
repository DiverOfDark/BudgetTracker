using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class BurstWidgetViewModel : WidgetViewModel
    {
        public bool ExemptTransfers { get; }

        public BurstWidgetViewModel(string providerName, string accountName, ObjectRepository repository,
            TableViewModel vm, bool exemptTransfers) : base(null, null)
        {
            ExemptTransfers = exemptTransfers;
            var column = repository.Set<MoneyColumnMetadataModel>().First(v =>
                v.Provider == providerName &&
                (v.AccountName == accountName ||
                 v.UserFriendlyName == accountName));

            RootNode = LoadData(column, repository, vm);
        }

        public Node RootNode { get; set; }

        public BurstWidgetViewModel(WidgetModel model, ObjectRepository repository, TableViewModel vm) :
            base(model, new BurstWidgetSettings(model.Properties.ToDictionary(v => v.Key, v => v.Value)))
        {
            var burstWidgetSettings = (BurstWidgetSettings) Settings;
            
            var column = repository.Set<MoneyColumnMetadataModel>().First(v =>
                v.Provider == burstWidgetSettings.ProviderName &&
                (v.AccountName == burstWidgetSettings.AccountName ||
                 v.UserFriendlyName == burstWidgetSettings.AccountName));

            RootNode = LoadData(column, repository, vm);
        }

        private Node LoadData(MoneyColumnMetadataModel column, ObjectRepository repository, TableViewModel vm)
        {
            var cell = vm.Values.FirstOrDefault()?.Cells?.GetValueOrDefault(column);
            var cellValue = ExemptTransfers ? cell?.AdjustedValue : cell?.Value;
            if (cellValue == null || double.IsNaN(cellValue.Value))
            {
                return null;
            }
            
            if (column.IsComputed && column.ChartList.Any())
            {
                var columnsToChart = repository.Set<MoneyColumnMetadataModel>().Where(v =>
                    column.ChartList.Contains(v.Provider + "/" + v.AccountName) ||
                    column.ChartList.Contains(v.UserFriendlyName)).ToList();

                var children = columnsToChart.Select(v => LoadData(v, repository, vm)).Where(v => v != null).ToList();

                return new Node
                {
                    Title = cell.Column.UserFriendlyName,
                    Amount = cellValue,
                    AmountFormatted = cellValue.Value.ToString("N2") + " " + cell.Ccy,
                    Children = children
                };
            }

            return new Node
            {
                Title = cell.Column.Provider + "/" + cell.Column.UserFriendlyName,
                Amount = cellValue,
                AmountFormatted = cellValue.Value.ToString("N2") + " " + cell.Ccy
            };
        }

        public override string TemplateName => WidgetExtensions.AsPath("~/Views/Widget/Widgets/BurstWidget.cshtml");
        public override int Columns => 12;
        public override int Rows => 4;

        public class Node
        {
            public string Title { get; set; }
            public double? Amount { get; set; }
            public string AmountFormatted { get; set; }
            public List<Node> Children { get; set; }
        }
    }
}