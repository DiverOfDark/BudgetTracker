using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class TableRowViewModel
    {
        public TableRowViewModel(IList<MoneyStateModel> item, List<MoneyColumnMetadataModel> headers, Dictionary<string, MoneyColumnMetadataModel> headersCached, Dictionary<MoneyColumnMetadataModel, Dictionary<DateTime, double>> paymentsToExempt)
        {
            When = item.Select(v => v.When).Max();
            Cells = new Dictionary<MoneyColumnMetadataModel, CalculatedResult>(); 
            
            foreach (var h in headers)
            {
                if (h.IsComputed)
                {
                    Cells[h] = CalculatedResult.FromComputed(headersCached, h, Cells);
                }
                else
                {
                    var money = item.Where(v => v.Provider == h.Provider && v.AccountName == h.AccountName).OrderByDescending(v=>v.When).FirstOrDefault();
                    if (money != null)
                    {
                        var adj = paymentsToExempt.GetValueOrDefault(h)?.GetValueOrDefault(money.When.Date.AddDays(-1)) ?? 0;

                        Cells[h] = CalculatedResult.FromMoney(h, money, adj);
                    }
                }
            }
        }
        
        public DateTime When { get; }
        
        public Dictionary<MoneyColumnMetadataModel, CalculatedResult> Cells { get; }
        public TableRowViewModel Previous { get; set; }
    }
}