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
        
        public TableViewModel(ObjectRepository repository, bool exemptTransfers)
        {
            Headers = repository.Set<MoneyColumnMetadataModel>().SortColumns().ToList();

            Dictionary<string, MoneyColumnMetadataModel> headersCached = new Dictionary<string, MoneyColumnMetadataModel>();

            foreach (var h in Headers)
            {
                if (!string.IsNullOrWhiteSpace(h.UserFriendlyName))
                    headersCached[h.UserFriendlyName] = h;

                headersCached[h.Provider + "/" + h.AccountName] = h;
            }

            var paymentsToExemptSource = exemptTransfers
                ? repository.Set<PaymentModel>().Where(v => v.Kind == PaymentKind.Transfer).ToList()
                : Enumerable.Empty<PaymentModel>();

            var paymentsToExempt = paymentsToExemptSource.GroupBy(v => v.Column).ToDictionary(v => v.Key, v => v.ToList());
            
            Values = repository.Set<MoneyStateModel>()
                .GroupBy(x => x.When.Date)
                .OrderByDescending(v => v.Key)
                .Select(v => new TableRowViewModel(v.ToList(), Headers, headersCached, paymentsToExempt))
                .ToList();

            for (int i = 0; i < Values.Count - 1; i++)
            {
                var row = Values[i];
                row.Previous = Values[i + 1];
                foreach (var value in row.Cells)
                {
                    var previous = Values[i + 1];
                    value.PreviousValue = previous.Cells.FirstOrDefault(v => v.Column == value.Column);
                }
            }

            var markedAsOkCells = Enumerable.Empty<MoneyColumnMetadataModel>();
            for (int i = Values.Count - 1; i >= 0; i--)
            {
                var row = Values[i];

                var toAddMissing = markedAsOkCells.Except(row.Cells.Select(v => v.Column)).ToList();
                
                foreach(var item in toAddMissing)
                {
                    row.Cells.Add(CalculatedResult.Empty(item));
                }

                markedAsOkCells = row.Cells.Where(v => v.Value != null && double.IsNaN(v.Value.Value))
                    .Select(v => v.Column).ToList();
            }

            foreach (var rowViewModel in Values)
            {
                rowViewModel.CalculateItems();
            }
        }

        public bool ShowAll { get; set; }
        public bool ShowControls { get; set; }
        public bool ExemptTransfers { get; set; }
        public bool ShowDelta { get; set; }

        public List<MoneyColumnMetadataModel> Headers { get; private set; }
        public List<TableRowViewModel> Values { get; private set; }
    }
}