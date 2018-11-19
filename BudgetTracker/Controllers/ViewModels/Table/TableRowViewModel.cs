using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class TableRowViewModel
    {
        public TableRowViewModel(IList<MoneyStateModel> item, List<MoneyColumnMetadataModel> headers, Dictionary<string, MoneyColumnMetadataModel> headersCached, Dictionary<MoneyColumnMetadataModel, List<PaymentModel>> paymentsToExempt)
        {
            When = item.OrderByDescending(v=>v.When).Select(v => v.When).FirstOrDefault();
            Cells = new List<CalculatedResult>();

            foreach (var h in headers)
            {
                if (h.IsComputed)
                {
                    Cells.Add(CalculateExpression.FromComputed(headersCached, h));
                }
                else
                {
                    var money = item.Where(v => v.Provider == h.Provider && v.AccountName == h.AccountName).OrderByDescending(v=>v.When).FirstOrDefault();
                    if (money != null)
                    {
                        double adj = 0;
                        
                        if (paymentsToExempt.ContainsKey(h))
                        {
                            foreach (var payment in paymentsToExempt[h])
                            {
                                if (money.When >= payment.When)
                                {
                                    adj -= payment.Amount;
                                }
                            }
                        }

                        Cells.Add(CalculatedResult.FromMoney(h, money, adj));
                    }
                }
            }
        }
        
        public DateTime When { get; }
        
        public List<CalculatedResult> Cells { get; }
        public TableRowViewModel Previous { get; set; }

        public void CalculateItems()
        {
            foreach (var item in Cells)
            {
                (item as CalculateExpression.ComputedCalculatedResult)?.EvalExpression(Cells);

                item.Tooltip += "\r\n => " + item.Value;
                if (item.FailedToResolve.Any())
                {
                    item.Tooltip += "\r\n\r\nНет данных по: " + String.Join(",\r\n", item.FailedToResolve);
                } 
            }
        }
    }
}