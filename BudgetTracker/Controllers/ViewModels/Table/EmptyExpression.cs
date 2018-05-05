using System.Collections.Generic;
using System.Linq;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class EmptyExpression : CalculateExpression
    {
        public EmptyExpression()
        {
            FailedToParse = Enumerable.Empty<string>();
        }
        
        public override void Evaluate(IList<CalculatedResult> dependencies)
        {
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression) => otherExpression;
        
        public override string ToString() => $"---";
    }
}