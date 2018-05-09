using System.ComponentModel;

namespace BudgetTracker.Model
{
    public enum RuleType
    {
        [DisplayName("Игнорировать")]
        Ignore,
        [DisplayName("Траты")]
        Money
    }
}