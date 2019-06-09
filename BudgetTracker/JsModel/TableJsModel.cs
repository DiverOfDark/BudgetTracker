using System.Collections.Generic;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.JsModel.Attributes;

namespace BudgetTracker.JsModel
{
    [ExportJsModel]
    public class TableJsModel
    {
        public TableJsModel(TableViewModel vm, string provider2, IEnumerable<string> providers)
        {
            Vm = vm;
            Provider = provider2;
            Providers = providers;
        }

        public TableViewModel Vm { get; set; }

        public IEnumerable<string> Providers { get; set; }

        public string Provider { get; set; }
    }
}