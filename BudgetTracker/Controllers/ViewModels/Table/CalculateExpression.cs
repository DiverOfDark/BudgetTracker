using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public abstract class CalculateExpression
    {
        public static CalculatedResult FromComputed(Dictionary<string, MoneyColumnMetadataModel> columns, MoneyColumnMetadataModel h)
        {
            var expression = Parse(columns, h.Function);
            return new ComputedCalculatedResult(expression);
        }

        public class ComputedCalculatedResult : CalculatedResult
        {
            private readonly CalculateExpression _expression;
            private bool _evaluated;

            public ComputedCalculatedResult(CalculateExpression expression)
            {
                _expression = expression;
                Tooltip = _expression.ToString();
            }

            public void EvalExpression(IList<CalculatedResult> dependencies)
            {
                if (_expression != null && !_evaluated)
                {
                    _evaluated = true;
                    _expression.Evaluate(dependencies);
                
                    Value = _expression.Value.Value;
                    FailedToResolve = _expression.Value.FailedToResolve;
                    Ccy = _expression.Value.Ccy;
                }
            }
        }

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
                    else if (function[0] == '(')
                    {
                        var counter = 1;
                        int i;
                        for (i = 1; i < function.Length && counter > 0; i++)
                        {
                            if (function[i] == '(')
                                counter++;
                            if (function[i] == ')')
                                counter--;
                        }

                        var sub = function.Substring(0, i);
                        var subExp = Parse(columns, sub.TrimStart('(').TrimEnd(')'));
                        currentExpression = currentExpression.TryApply(new ParenthesisExpression(subExp));
                        
                        function = function.Substring(i);
                    }
                    else 
                    {
                        var symbol = BinaryExpression.Symbols.FirstOrDefault(function.StartsWith);
                        if (symbol != null)
                        {
                            currentExpression = new BinaryExpression(symbol).TryApply(currentExpression);
                            function = function.Substring(symbol.Length);
                        }
                        else
                        {
                            return new FailedToParseExpression(function);
                        }
                    }
                }
            }
            catch
            {
                return new FailedToParseExpression(function);
            }

            return currentExpression;
        }

        public abstract void Evaluate(IList<CalculatedResult> dependencies);

        public CalculatedResult Value { get; protected set; }
        
        public abstract CalculateExpression TryApply(CalculateExpression otherExpression);
    }
}