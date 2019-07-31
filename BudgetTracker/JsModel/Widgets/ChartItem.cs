using System;
using BudgetTracker.JsModel.Attributes;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    [ExportJsModel]
    public class ChartItem
    {
        public DateTime When { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public string Ccy { get; set; }
    }
}