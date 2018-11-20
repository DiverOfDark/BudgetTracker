using System.Collections.Generic;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class FailedToParseExpression : CalculateExpression
    {
        private readonly string _function;

        public FailedToParseExpression(MoneyColumnMetadataModel model, string function)
        {
            _function = function;
            Value = CalculatedResult.ResolutionFail(model, function);
        }

        public override void Evaluate(IEnumerable<CalculatedResult> dependencies)
        {
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression) => throw new System.NotImplementedException();
        
        public override string ToString() => $"[<?>{_function}]";
    }
}