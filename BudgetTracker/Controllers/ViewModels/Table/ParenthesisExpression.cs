using System.Collections.Generic;
using BudgetTracker.JsModel;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class ParenthesisExpression : CalculateExpression
    {
        private readonly CalculateExpression _baseExpression;

        public ParenthesisExpression(CalculateExpression baseExpression)
        {
            _baseExpression = baseExpression;
        }

        public override void Evaluate(Dictionary<MoneyColumnMetadataJsModel, CalculatedResult> dependencies)
        {
            if (Value == null)
            {
                _baseExpression.Evaluate(dependencies);
                Value = _baseExpression.Value;
            }
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression) => _baseExpression.TryApply(otherExpression);

        protected override string ToStringImpl() => $"({_baseExpression})";
    }
}