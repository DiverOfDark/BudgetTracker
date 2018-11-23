using System;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class TableViewModelFactory
    {
        private readonly Lazy<TableViewModel> _vmWithTransfers;
        
        public TableViewModelFactory(ObjectRepository repository)
        {
            _vmWithTransfers = new Lazy<TableViewModel>(()=>new TableViewModel(repository));
        }
        
        public TableViewModel GetVM()
        {
            var source = _vmWithTransfers;

            return new TableViewModel(source.Value);
        }
    }
}