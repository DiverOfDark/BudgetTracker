using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class TableViewModel
    {
        public TableViewModel(TableViewModel source)
        {
            Headers = source.Headers;
            Values = source.Values;
        }
        
        public TableViewModel(ObjectRepository repository)
        {
            Headers = repository.Set<MoneyColumnMetadataModel>().SortColumns().ToList();

            Dictionary<string, MoneyColumnMetadataModel> headersCached = new Dictionary<string, MoneyColumnMetadataModel>();

            foreach (var h in Headers)
            {
                if (!string.IsNullOrWhiteSpace(h.UserFriendlyName))
                    headersCached[h.UserFriendlyName] = h;

                headersCached[h.Provider + "/" + h.AccountName] = h;
            }

            var paymentsToExempt = repository.Set<PaymentModel>().Where(v => v.Kind == PaymentKind.Transfer).ToList().GroupBy(v => v.Column)
                .ToDictionary(v => v.Key, v =>
                {
                    var d = new Dictionary<DateTime, double>();
                    foreach (var item in v)
                    {
                        d[item.When.Date] = d.GetValueOrDefault(item.When.Date, 0) + item.Amount;
                    }
                    return d;
                });

            var rows = repository.Set<MoneyStateModel>()
                .GroupBy(x => x.When.Date)
                .OrderByDescending(v => v.Key)
                .ToList();

            var end = rows.Select(v => (DateTime?)v.Key.Date).FirstOrDefault();

            foreach (var col in paymentsToExempt)
            {
                var cumulative = 0.0;
                DateTime start = col.Value.Keys.Min();
                for(; start <= (end ?? start); start = start.AddDays(1))
                {
                    var curValue = col.Value.GetValueOrDefault(start, 0);
                    if (Math.Abs(cumulative + curValue) >= 0.0001 )
                    {
                        col.Value[start] = -(curValue + cumulative);
                        cumulative += curValue;
                    }
                }
            }
            
            Values = rows
                .Select(v => new TableRowViewModel(v.ToList(), Headers, headersCached, paymentsToExempt))
                .ToList();

            for (int i = 0; i < Values.Count - 1; i++)
            {
                var row = Values[i];
                row.Previous = Values[i + 1];
                foreach (var value in row.CalculatedCells.Values)
                {
                    var previous = Values[i + 1];
                    value.PreviousValue = previous.CalculatedCells.GetValueOrDefault(value.Column);
                }
            }

            var markedAsOkCells = Enumerable.Empty<MoneyColumnMetadataModel>();
            for (int i = Values.Count - 1; i >= 0; i--)
            {
                var row = Values[i];

                var toAddMissing = markedAsOkCells.Except(row.CalculatedCells.Keys).ToList();
                
                foreach(var item in toAddMissing)
                {
                    row.CalculatedCells.Add(item, CalculatedResult.Empty(item));
                }

                markedAsOkCells = row.CalculatedCells.Values.Where(v => !(v is ExpressionCalculatedResult) && v.Value != null && double.IsNaN(v.Value.Value))
                    .Select(v => v.Column).ToList();
            }

            var columnsToCheck = Values.SelectMany(v => v.CalculatedCells.Keys).Distinct()
                .Where(v => !v.IsComputed).ToList();
            for (int i = Values.Count - 1; i >= 0; i--)
            {
                var row = Values[i];

                columnsToCheck = columnsToCheck.Except(row.CalculatedCells.Keys).ToList();
                
                foreach(var item in columnsToCheck)
                {
                    row.CalculatedCells.Add(item, CalculatedResult.Empty(item));
                }
            }
        }

        public List<MoneyColumnMetadataModel> Headers { get; private set; }
        public List<TableRowViewModel> Values { get; set; }
    }
}