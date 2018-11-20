using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class EmptyExpression : CalculateExpression
    {
        public EmptyExpression(MoneyColumnMetadataModel model)
        {
            Value = CalculatedResult.Empty(model);
        }
        
        public override void Evaluate(IEnumerable<CalculatedResult> dependencies)
        {
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression) => otherExpression;
        
        public override string ToString() => $"---";
    }
}