using System.Collections.Generic;
using BudgetTracker.JsModel;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public abstract class CalculateExpression
    {
        public abstract void Evaluate(Dictionary<MoneyColumnMetadataJsModel, CalculatedResult> dependencies);

        public CalculatedResult Value { get; protected set; }
        
        public abstract CalculateExpression TryApply(CalculateExpression otherExpression);

        protected abstract string ToStringImpl();

        private string _toStringCalculated;
        public sealed override string ToString() => _toStringCalculated ?? (_toStringCalculated = ToStringImpl());
    }
}