using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void Evaluate(IEnumerable<CalculatedResult> dependencies)
        {
            if (!_evaluated)
            {
                _evaluated = true;
                
                var matchedDependency = dependencies.FirstOrDefault(v => v.Column == _column);

                Value = matchedDependency?.Value == null
                        ? CalculatedResult.ResolutionFail(_column, _column.Provider + "/" + _column.AccountName)
                        : matchedDependency;
            }
        }
        
        public override CalculateExpression TryApply(CalculateExpression otherExpression) => throw new System.NotImplementedException();

        public override string ToString() => $"[{_column.Provider}/{_column.UserFriendlyName ?? _column.AccountName}]({Value})";
    }
}