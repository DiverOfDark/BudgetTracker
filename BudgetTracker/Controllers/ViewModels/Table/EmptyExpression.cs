using System.Collections.Generic;
using BudgetTracker.JsModel;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class EmptyExpression : CalculateExpression
    {
        public EmptyExpression(MoneyColumnMetadataJsModel model)
        {
            Value = CalculatedResult.Empty(model);
        }
        
        public override void Evaluate(Dictionary<MoneyColumnMetadataJsModel, CalculatedResult> dependencies)
        {
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression) => otherExpression;
        
        protected override string ToStringImpl() => "---";
    }
}