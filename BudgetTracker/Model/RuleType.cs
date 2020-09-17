using BudgetTracker.JsModel.Attributes;

namespace BudgetTracker.Model
{
    [ExportJsModel]
    public enum RuleType
    {
        [JsDisplayName("Игнорировать")]
        Ignore,
        [JsDisplayName("Траты")]
        Money
    }
}