using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class BinaryExpression : CalculateExpression
    {
        private readonly string _symbol;
        public static readonly string[] Symbols = { "/", "*", "+", "-", "??" }; 

        public BinaryExpression(string symbol)
        {
            _symbol = symbol;
        }

        public CalculateExpression Left { get; set; }
        public CalculateExpression Right { get; set; }

        public override void Evaluate(IList<CalculatedResult> dependencies)
        {
            if (Left == null || Right == null)
            {
                Value = CalculatedResult.Empty(null);
                Value.FailedToResolve = new[] {_symbol};
                
                return;
            }

            Left.Evaluate(dependencies);
            Right.Evaluate(dependencies);

            var leftValue = Left.Value.Value;
            var rightValue = Right.Value.Value;

            bool leftIsNan = leftValue != null && double.IsNaN(leftValue.Value);
            bool rightIsNan = rightValue != null && double.IsNaN(rightValue.Value);

            var result = CalculatedResult.Empty(null);
            
            if (leftIsNan && !rightIsNan)
            {
                leftValue = 0;
            }

            if (rightIsNan && !leftIsNan)
            {
                rightValue = 0;
            }
            
            result.FailedToResolve = Left.Value.FailedToResolve.Concat(Right.Value.FailedToResolve).ToList();

            switch (_symbol)
            {
                case "??":
                    result.Value = leftIsNan ? rightValue : leftValue;
                    result.Ccy = SelectCcy(Left.Value.Ccy, Right.Value.Ccy);
                    result.FailedToResolve = leftIsNan ? Right.Value.FailedToResolve : Left.Value.FailedToResolve;
                    break;
                case "+":
                    result.Value = leftValue + rightValue;
                    result.Ccy = SelectCcy(Left.Value.Ccy, Right.Value.Ccy);
                    break;
                case "-":
                    result.Value = leftValue - rightValue;
                    result.Ccy = SelectCcy(Left.Value.Ccy, Right.Value.Ccy);
                    break;
                
                case "*":
                    result.Value = leftValue * rightValue;
                    result.Ccy = Left.Value.Ccy;

                    // TODO ccy?
                    break;
                case "/":
                    result.Value = leftValue / rightValue;

                    // TODO ccy?
                    break;

                default:
                    result.Value = null;
                    result.FailedToResolve = new[] {_symbol};
                    break;
            }

            Value = result;
        }

        private string SelectCcy(string first, string second)
        {
            if (first == null || second == null)
                return first ?? second;

            return first.Length < second.Length ? first : second;
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression)
        {
            if (Left == null)
            {
                Left = otherExpression;
            } else if (Right == null)
            {
                Right = otherExpression;
            }
            else throw new NotSupportedException();

            return this;
        }
        
        public override string ToString() => $"{Left} {_symbol} {Right}";
    }
}