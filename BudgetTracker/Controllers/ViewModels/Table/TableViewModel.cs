using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class TableViewModel
    {
        private static TableViewModel _instance;
        
        public static TableViewModel GetCachedViewModel(bool showAll, bool showControls, bool showDelta, ObjectRepository repository)
        {
            if (_instance == null)
            {
                lock(typeof(TableViewModel))
                {
                    if (_instance == null)
                    {
                        void RepositoryOnModelChanged(ModelChangedEventArgs args)
                        {
                            if (args.Source is MoneyStateModel || args.Source is MoneyColumnMetadataModel)
                            {
                                _instance = null;
                                repository.ModelChanged -= RepositoryOnModelChanged;
                            }
                        }

                        repository.ModelChanged += RepositoryOnModelChanged;
                        
                        _instance = new TableViewModel(repository);
                    }
                }
            }
            
            return new TableViewModel(_instance)
            {
                ShowAll = showAll,
                ShowControls = showControls,
                ShowDelta = showDelta
            };
        }

        private TableViewModel(TableViewModel source)
        {
            Headers = source.Headers;
            Values = source.Values;
        }
        
        private TableViewModel(
            ObjectRepository repository)
        {
            Headers = repository.Set<MoneyColumnMetadataModel>().SortColumns().ToList();

            Dictionary<string, MoneyColumnMetadataModel> headersCached = new Dictionary<string, MoneyColumnMetadataModel>();

            foreach (var h in Headers)
            {
                if (!string.IsNullOrWhiteSpace(h.UserFriendlyName))
                    headersCached[h.UserFriendlyName] = h;

                headersCached[h.Provider + "/" + h.AccountName] = h;
            }

            Values = repository.Set<MoneyStateModel>()
                .GroupBy(x => x.When.Date)
                .OrderByDescending(v => v.Key)
                .Select(v => new TableRowViewModel(v.ToList(), Headers, headersCached))
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

        public bool ShowAll { get; private set; }
        public bool ShowControls { get; private set; }
        public bool ShowDelta { get; private set; }

        public List<MoneyColumnMetadataModel> Headers { get; private set; }
        public List<TableRowViewModel> Values { get; private set; }
    }
}