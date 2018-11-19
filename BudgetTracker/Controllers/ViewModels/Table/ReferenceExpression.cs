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

        public override void Evaluate(IList<CalculatedResult> dependencies)
        {
            if (!_evaluated)
            {
                _evaluated = true;
                
                var matchedDependency = dependencies.FirstOrDefault(v => v.Column == _column);

                if (matchedDependency == null)
                {
                    Value = CalculatedResult.Empty(_column);
                    Value.FailedToResolve = new[] {_column.Provider + "/" + _column.AccountName};
                }
                else 
                {
                    (matchedDependency as ComputedCalculatedResult)?.EvalExpression(dependencies);

                    if (matchedDependency.Value != null)
                    {
                        Value = matchedDependency;
                    }
                    else
                    {
                        Value = CalculatedResult.Empty(_column);
                        Value.FailedToResolve = new[] {_column.Provider + "/" + _column.AccountName};
                    }
                }
            }
        }
        
        public override CalculateExpression TryApply(CalculateExpression otherExpression) => throw new System.NotImplementedException();

        public override string ToString() => $"[{_column.Provider}/{_column.UserFriendlyName ?? _column.AccountName}]({Value})";
    }
}