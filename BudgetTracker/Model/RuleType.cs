using BudgetTracker.JsModel.Attributes;

namespace BudgetTracker.Model
{
    public enum RuleType
    {
        [JsDisplayName("Игнорировать")]
        Ignore,
        [JsDisplayName("Траты")]
        Money
    }
}