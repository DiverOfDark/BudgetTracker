using System.Collections.Generic;
using BudgetTracker.JsModel.Attributes;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    [ExportJsModel]
    public class ChartValue
    {
        public string Label { get; set; }

        public List<ChartItem> Values { get; set; }
    }
}