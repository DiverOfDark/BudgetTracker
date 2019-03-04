using System.Collections.Generic;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class ReferenceExpression : CalculateExpression
    {
        private readonly MoneyColumnMetadataModel _column;

        private bool _evaluated;
        
        public ReferenceExpression(MoneyColumnMetadataModel column)
        {
            _column = column;
        }

        public override void Evaluate(Dictionary<MoneyColumnMetadataModel, CalculatedResult> dependencies)
        {
            if (!_evaluated)
            {
                _evaluated = true;

                dependencies.TryGetValue(_column, out var matchedDependency);

                Value = matchedDependency?.Value == null
                        ? CalculatedResult.ResolutionFail(_column, _column.Provider + "/" + _column.AccountName)
                        : matchedDependency;
            }
        }
        
        public override CalculateExpression TryApply(CalculateExpression otherExpression) => throw new System.NotImplementedException();

        protected override string ToStringImpl() => $"[{_column.Provider}/{_column.UserFriendlyName ?? _column.AccountName}]({Value})";
    }
}