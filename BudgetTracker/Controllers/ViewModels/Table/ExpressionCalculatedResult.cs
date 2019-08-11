using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.JsModel;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class ExpressionCalculatedResult : CalculatedResult
    {
        private readonly Dictionary<MoneyColumnMetadataJsModel, CalculatedResult> _deps;
        private readonly CalculateExpression _expression;
        private bool _evaluated;

        private double? _value;
        private double _adjustment;
        private string _ccy;
        private string _tooltip;
        private IEnumerable<string> _failedToResolve;
        
        public ExpressionCalculatedResult(Dictionary<string, MoneyColumnMetadataJsModel> columns, MoneyColumnMetadataJsModel h, Dictionary<MoneyColumnMetadataJsModel, CalculatedResult> deps) : base(h)
        {
            _deps = deps;
            var expression = Parse(columns, h, h.Function);

            _expression = expression;
        }

        public override string Tooltip
        {
            get
            {
                EvalExpression();
                return _tooltip;
            }
        }

        public override double? Value
        {
            get
            {
                EvalExpression();
                return _value;
            }
        }

        public override double Adjustment
        {
            get
            {
                EvalExpression();
                return _adjustment;
            }
        }

        public override string Ccy
        {
            get
            {
                EvalExpression();
                return _ccy;
            }
        }

        public override IEnumerable<string> FailedToResolve
        {
            get
            {
                EvalExpression();
                return _failedToResolve;
            }
        }

        private static CalculateExpression Parse(Dictionary<string, MoneyColumnMetadataJsModel> columns, MoneyColumnMetadataJsModel column, string function)
        {
            CalculateExpression currentExpression = new EmptyExpression(column);
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
                        var subExp = Parse(columns, column, sub.TrimStart('(').TrimEnd(')'));
                        currentExpression = currentExpression.TryApply(new ParenthesisExpression(subExp));
                        
                        function = function.Substring(i);
                    }
                    else if (function[0] == '{')
                    {
                        var constant = function.Substring(1, function.IndexOf('}') - 1);

                        function = function.Remove(0, function.IndexOf('}') + 1);

                        currentExpression = currentExpression.TryApply(new ConstantExpression(column, constant));
                    } 
                    else 
                    {
                        var symbol = BinaryExpression.Symbols.FirstOrDefault(function.StartsWith);
                        if (symbol != null)
                        {
                            currentExpression = new BinaryExpression(column, symbol).TryApply(currentExpression);
                            function = function.Substring(symbol.Length);
                        }
                        else
                        {
                            return new FailedToParseExpression(column, function);
                        }
                    }
                }
            }
            catch
            {
                return new FailedToParseExpression(column, function);
            }

            return currentExpression;
        }

        public void EvalExpression()
        {
            if (_expression != null && !_evaluated)
            {
                _evaluated = true;
                _expression.Evaluate(_deps);
                
                _value = _expression.Value.Value;
                _failedToResolve = _expression.Value.FailedToResolve;
                _ccy = _expression.Value.Ccy;
                _adjustment = _expression.Value.Adjustment;
                
                _tooltip += _expression + "\r\n => " + Value + " + " + Adjustment + " => " + (Value + Adjustment);
                if (FailedToResolve.Any())
                {
                    _tooltip += "\r\n\r\nНет данных по: " + String.Join(",\r\n", FailedToResolve);
                } 
            }
        }
    }
}