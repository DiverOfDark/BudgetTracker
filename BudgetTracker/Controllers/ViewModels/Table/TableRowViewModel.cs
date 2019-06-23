using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BudgetTracker.JsModel;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;
using Newtonsoft.Json;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    [ExportJsModel]
    public class TableRowViewModel
    {
        public TableRowViewModel(IList<MoneyStateModel> item, List<MoneyColumnMetadataJsModel> headers, Dictionary<string, MoneyColumnMetadataJsModel> headersCached, Dictionary<MoneyColumnMetadataJsModel, Dictionary<DateTime, double>> paymentsToExempt)
        {
            When = item.Select(v => v.When).Max();
            Cells = new List<CalculatedResult>();

            var grouped = item.GroupBy(v => v.Provider).ToDictionary(v => v.Key,
                v => v.GroupBy(t => t.AccountName)
                    .ToDictionary(s => s.Key, s => s.OrderByDescending(t => t.When).First()));
            
            CalculatedCells = new Dictionary<MoneyColumnMetadataJsModel, CalculatedResult>();
            foreach (var h in headers)
            {
                CalculatedResult cell = null;
                if (h.IsComputed)
                {
                    cell = CalculatedResult.FromComputed(headersCached, h, CalculatedCells);
                }
                else
                {
                    var money = grouped.GetValueOrDefault(h.Provider)?.GetValueOrDefault(h.AccountName);
                    if (money != null)
                    {
                        var adj = paymentsToExempt.GetValueOrDefault(h)?.GetValueOrDefault(money.When.Date.AddDays(-1)) ?? 0;

                        cell = CalculatedResult.FromMoney(h, money, adj);
                    }
                }

                Cells.Add(cell);

                if (cell != null)
                {
                    CalculatedCells[h] = cell;
                }
            }
        }
        
        public DateTime When { get; }

        [JsonIgnore] public Dictionary<MoneyColumnMetadataJsModel, CalculatedResult> CalculatedCells { get; }

        public List<CalculatedResult> Cells {get; }
        
        [JsonIgnore]
        public TableRowViewModel Previous { get; set; }
    }
}