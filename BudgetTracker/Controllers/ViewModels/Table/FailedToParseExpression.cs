using System.Collections.Generic;
using BudgetTracker.JsModel;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class FailedToParseExpression : CalculateExpression
    {
        private readonly string _function;

        public FailedToParseExpression(MoneyColumnMetadataJsModel model, string function)
        {
            _function = function;
            Value = CalculatedResult.ResolutionFail(model, function);
        }

        public override void Evaluate(Dictionary<MoneyColumnMetadataJsModel, CalculatedResult> dependencies)
        {
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression) => throw new System.NotImplementedException();
        
        protected override string ToStringImpl() => $"[<?>{_function}]";
    }
}