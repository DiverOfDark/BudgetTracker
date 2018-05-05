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
                    Value = 0;
                    FailedToParse = new[] {_column.Provider + "/" + _column.AccountName};
                }
                else 
                {
                    matchedDependency.EvalExpression(dependencies);

                    if (matchedDependency.Value != null)
                    {
                        var value = matchedDependency.Value;
                        var failedToResolve = matchedDependency.FailedToResolve;
                        if (value != null && double.IsNaN(value.Value))
                        {
                            value = 0;
                            failedToResolve = Enumerable.Empty<string>();
                        }

                        Value = value;
                        FailedToParse = failedToResolve;
                        Ccy = matchedDependency.Ccy;
                    }
                    else
                    {
                        Value = 0;
                        FailedToParse = new[] {_column.Provider + "/" + _column.AccountName};
                        Ccy = matchedDependency.Ccy;
                    }
                }
            }
        }
        
        public override CalculateExpression TryApply(CalculateExpression otherExpression) => throw new System.NotImplementedException();

        public override string ToString() => $"[{_column.Provider}/{_column.UserFriendlyName ?? _column.AccountName}]({Value?.ToString("F2")})";
    }
}