using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.JsModel;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class BurstWidgetViewModel : WidgetViewModel
    {
        public bool ExemptTransfers { get; }

        public BurstWidgetViewModel(string providerName, string accountName,
            TableViewModel vm, bool exemptTransfers) : base(null, null)
        {
            ExemptTransfers = exemptTransfers;
            var column = vm.Headers.First(v =>
                v.Provider == providerName &&
                (v.AccountName == accountName ||
                 v.UserFriendlyName == accountName));

            RootNode = LoadData(column, vm);
        }

        public Node RootNode { get; set; }

        public BurstWidgetViewModel(WidgetModel model, TableViewModel vm) :
            base(model, new BurstWidgetSettings(model.Properties.ToDictionary(v => v.Key, v => v.Value)))
        {
            var burstWidgetSettings = (BurstWidgetSettings) Settings;
            
            var column = vm.Headers.First(v =>
                v.Provider == burstWidgetSettings.ProviderName &&
                (v.AccountName == burstWidgetSettings.AccountName ||
                 v.UserFriendlyName == burstWidgetSettings.AccountName));

            RootNode = LoadData(column, vm);
        }

        private Node LoadData(MoneyColumnMetadataJsModel column, TableViewModel vm)
        {
            var cell = vm.Values.FirstOrDefault()?.CalculatedCells?.GetValueOrDefault(column);
            var cellValue = ExemptTransfers ? cell?.AdjustedValue : cell?.Value;
            if (cellValue == null || cellValue == 0 || double.IsNaN(cellValue.Value))
            {
                return null;
            }
            
            if (column.IsComputed && column.ChartList.Any())
            {
                var columnsToChart = vm.Headers.Where(v =>
                    column.ChartList.Contains(v.Provider + "/" + v.AccountName) ||
                    column.ChartList.Contains(v.UserFriendlyName)).ToList();

                var children = columnsToChart.Select(v => LoadData(v, vm)).Where(v => v != null).ToList();

                if (children.Count == 0)
                {
                    return null;
                }

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

        public override int Columns => 12;
        public override int Rows => 4;

        [ExportJsModel]
        public class Node
        {
            public string Title { get; set; }
            public double? Amount { get; set; }
            public string AmountFormatted { get; set; }
            public List<Node> Children { get; set; }
        }
    }
}