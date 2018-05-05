using System.Collections.Generic;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class FailedToParseExpression : CalculateExpression
    {
        private readonly string _function;

        public FailedToParseExpression(string function)
        {
            _function = function;
            FailedToParse = new[] {function};
        }

        public override void Evaluate(IList<CalculatedResult> dependencies)
        {
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression) => throw new System.NotImplementedException();
        
        public override string ToString() => $"[<?>{_function}]";
    }
}