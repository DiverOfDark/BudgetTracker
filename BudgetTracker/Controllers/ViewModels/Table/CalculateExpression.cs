using System.Collections.Generic;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public abstract class CalculateExpression
    {
        public abstract void Evaluate(IList<CalculatedResult> dependencies);

        public double? Value { get; protected set; }
        
        public string Ccy { get; protected set; }
        
        public IEnumerable<string> FailedToParse { get; protected set; } 
        
        public abstract CalculateExpression TryApply(CalculateExpression otherExpression);
    }
}