using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BudgetTracker.Model;
using Newtonsoft.Json;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class TableRowViewModel
    {
        public TableRowViewModel(IList<MoneyStateModel> item, List<MoneyColumnMetadataModel> headers, Dictionary<string, MoneyColumnMetadataModel> headersCached, Dictionary<MoneyColumnMetadataModel, Dictionary<DateTime, double>> paymentsToExempt)
        {
            When = item.Select(v => v.When).Max();
            CalculatedCells = new Dictionary<MoneyColumnMetadataModel, CalculatedResult>(); 
            
            foreach (var h in headers)
            {
                if (h.IsComputed)
                {
                    CalculatedCells[h] = CalculatedResult.FromComputed(headersCached, h, CalculatedCells);
                }
                else
                {
                    var money = item.Where(v => v.Provider == h.Provider && v.AccountName == h.AccountName).OrderByDescending(v=>v.When).FirstOrDefault();
                    if (money != null)
                    {
                        var adj = paymentsToExempt.GetValueOrDefault(h)?.GetValueOrDefault(money.When.Date.AddDays(-1)) ?? 0;

                        CalculatedCells[h] = CalculatedResult.FromMoney(h, money, adj);
                    }
                }
            }
        }
        
        public DateTime When { get; }
        
        [JsonIgnore]
        public Dictionary<MoneyColumnMetadataModel, CalculatedResult> CalculatedCells { get; }

        public Dictionary<Guid, CalculatedResult> Cells => CalculatedCells.ToDictionary(v => v.Key.Id, v => v.Value);
        
        [JsonIgnore]
        public TableRowViewModel Previous { get; set; }
    }
}