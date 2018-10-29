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

        public override void Evaluate(IList<CalculatedResult> dependencies)
        {
            _baseExpression.Evaluate(dependencies);
            Ccy = _baseExpression.Ccy;
            Value = _baseExpression.Value;
            FailedToParse = _baseExpression.FailedToParse;
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression) => _baseExpression.TryApply(otherExpression);

        public override string ToString() => $"({_baseExpression})";
    }
}