using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BudgetTracker.JsModel;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    [ExportJsModel]
    public class TableRowViewModel
    {
        public TableRowViewModel(IList<MoneyStateModel> item, List<MoneyColumnMetadataJsModel> headers, Dictionary<string, MoneyColumnMetadataJsModel> headersCached, Dictionary<MoneyColumnMetadataJsModel, Dictionary<DateTime, double>> paymentsToExempt)
        {
            var minDate = item.Select(v => v.When).Min();
            var maxDate = item.Select(v => v.When).Max();
            When = minDate.AddSeconds((maxDate - minDate).TotalSeconds / 2);
            
            When = When.Date.AddHours(12);

            var grouped = item.GroupBy(v => v.Column).ToDictionary(v => v.Key,
                    s => s.OrderByDescending(t => t.When).First());
            
            CalculatedCells = new Dictionary<MoneyColumnMetadataJsModel, CalculatedResult>();
            foreach (var h in headers)
            {
                CalculatedResult cell = null;
                var money = grouped.GetValueOrDefault(h.Column);

                if (money == null && h.IsComputed)
                {
                    cell = CalculatedResult.FromComputed(headersCached, h, CalculatedCells);
                }
                else if (money != null)
                {
                    var adj = paymentsToExempt.GetValueOrDefault(h)?.GetValueOrDefault(money.When.Date.AddDays(-1)) ?? 0;

                    cell = CalculatedResult.FromMoney(h, money, adj);
                }

                CalculatedCells[h] = cell ?? CalculatedResult.Missing(h);
            }

            // make a copy of dictionary to make it editable and to not lost references to dependencies
            CalculatedCells = CalculatedCells.ToDictionary(v => v.Key, v => v.Value);
        }
        
        public DateTime When { get; }

        [JsonIgnore] public Dictionary<MoneyColumnMetadataJsModel, CalculatedResult> CalculatedCells { get; }

        [UsedImplicitly, Obsolete("Should be used by js only", true)]
        public IEnumerable<CalculatedResult> Cells => CalculatedCells.Values;
        
        [JsonIgnore]
        public TableRowViewModel Previous { get; set; }
    }
}