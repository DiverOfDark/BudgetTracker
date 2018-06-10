using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class CalculatedResult
    {
        public static CalculatedResult FromComputed(Dictionary<string, MoneyColumnMetadataModel> columns, MoneyColumnMetadataModel h) => new CalculatedResult
        {
            Column = h,
            _expression = Parse(columns, h.Function)
        };

        public static CalculatedResult FromMoney(MoneyColumnMetadataModel h, MoneyStateModel money, double adjustment) => new CalculatedResult
        {
            Ccy = money.Ccy,
            Column = h,
            Money = money,
            Value = money.Amount + adjustment,
            Tooltip = $"{(money.Amount + adjustment).ToString(CultureInfo.CurrentCulture)}({money.Amount.ToString(CultureInfo.CurrentCulture)} + {adjustment.ToString(CultureInfo.CurrentCulture)})"
        };

        public static CalculatedResult Empty(MoneyColumnMetadataModel item) => new CalculatedResult
        {
            Ccy = null,
            Column = item,
            Value = double.NaN
        };

        private bool _evaluated;
        private CalculateExpression _expression;
        
        private CalculatedResult() { }

        public MoneyColumnMetadataModel Column { get; private set; }

        public MoneyStateModel Money { get; private set;}

        public double? Value { get; private set; }
        public IEnumerable<string> FailedToResolve { get; private set; } = Enumerable.Empty<string>();

        public string Tooltip { get; set; }

        public string Ccy { get; set; }

        public CalculatedResult PreviousValue { get; set; }

        public double? DiffValue => Value - PreviousValue?.Value;

        public double? DiffPercentage => DiffValue / PreviousValue?.Value;
        
        public static CalculateExpression Parse(Dictionary<string, MoneyColumnMetadataModel> columns, string function)
        {
            CalculateExpression currentExpression = new EmptyExpression();
            try
            {
                while (function.Length > 0)
                {
                    function = function.Trim();

                    if (function[0] == '[')
                    {
                        var referenceName = function.Substring(1, function.IndexOf(']') - 1);

                        function = function.Remove(0, function.IndexOf(']') + 1);

                        var matchingColumn = columns[referenceName];
                        
                        currentExpression = currentExpression.TryApply(new ReferenceExpression(matchingColumn));
                    }
                    else if (BinaryExpression.Symbols.Contains(function[0]))
                    {
                        currentExpression = new BinaryExpression(function[0]).TryApply(currentExpression);
                        function = function.Substring(1);
                    }
                    else
                    {
                        return new FailedToParseExpression(function);
                    }
                }
            }
            catch
            {
                return new FailedToParseExpression(function);
            }

            return currentExpression;
        }

        public void EvalExpression(IList<CalculatedResult> dependencies)
        {
            if (_expression != null && !_evaluated)
            {
                _evaluated = true;
                _expression.Evaluate(dependencies);
                
                Value = _expression.Value;
                FailedToResolve = _expression.FailedToParse;
                Ccy = _expression.Ccy;
                Tooltip = _expression.ToString();
            }
        }

        public override string ToString()
        {
            if (_expression != null)
                return _expression.ToString();
            return $"[{Column.Provider}/{Column.AccountName}]({Value})";
        }
    }
}