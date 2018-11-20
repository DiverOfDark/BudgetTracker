using System.Collections.Generic;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public abstract class CalculateExpression
    {
        public abstract void Evaluate(IEnumerable<CalculatedResult> dependencies);

        public CalculatedResult Value { get; protected set; }
        
        public abstract CalculateExpression TryApply(CalculateExpression otherExpression);
    }
}