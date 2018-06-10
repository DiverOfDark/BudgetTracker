using System;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class TableViewModelFactory
    {
        private readonly Lazy<TableViewModel> _vmWithTransfers;
        private readonly Lazy<TableViewModel> _vmWithoutTransfers;
        
        public TableViewModelFactory(ObjectRepository repository)
        {
            _vmWithTransfers = new Lazy<TableViewModel>(()=>new TableViewModel(repository, false));
            _vmWithoutTransfers = new Lazy<TableViewModel>(()=>new TableViewModel(repository, true));
        }
        
        public TableViewModel GetVM(bool exemptTransfers)
        {
            var source = exemptTransfers ? _vmWithoutTransfers : _vmWithTransfers;

            return new TableViewModel(source.Value);
        }
    }
}