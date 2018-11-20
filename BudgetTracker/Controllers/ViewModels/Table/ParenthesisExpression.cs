using System.Collections.Generic;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class ParenthesisExpression : CalculateExpression
    {
        private readonly CalculateExpression _baseExpression;

        public ParenthesisExpression(CalculateExpression baseExpression)
        {
            _baseExpression = baseExpression;
        }

        public override void Evaluate(IEnumerable<CalculatedResult> dependencies)
        {
            if (Value == null)
            {
                _baseExpression.Evaluate(dependencies);
                Value = _baseExpression.Value;
            }
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression) => _baseExpression.TryApply(otherExpression);

        public override string ToString() => $"({_baseExpression})";
    }
}