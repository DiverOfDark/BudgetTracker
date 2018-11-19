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
                Value = null;
                FailedToParse = new[] {_symbol};
                
                return;
            }

            Left.Evaluate(dependencies);
            Right.Evaluate(dependencies);

            var leftValue = Left.Value;
            var rightValue = Right.Value;

            bool leftIsNan = leftValue != null && double.IsNaN(leftValue.Value);
            bool rightIsNan = rightValue != null && double.IsNaN(rightValue.Value);

            if (leftIsNan && !rightIsNan)
            {
                leftValue = 0;
            }

            if (rightIsNan && !leftIsNan)
            {
                rightValue = 0;
            }
            
            FailedToParse = Left.FailedToParse.Concat(Right.FailedToParse).ToList();

            switch (_symbol)
            {
                case "??":
                    Value = leftIsNan ? rightValue : leftValue;
                    Ccy = SelectCcy(Left.Ccy, Right.Ccy);
                    FailedToParse = leftIsNan ? Right.FailedToParse : Left.FailedToParse;
                    break;
                case "+":
                    Value = leftValue + rightValue;
                    Ccy = SelectCcy(Left.Ccy, Right.Ccy);
                    break;
                case "-":
                    Value = leftValue - rightValue;
                    Ccy = SelectCcy(Left.Ccy, Right.Ccy);
                    break;
                
                case "*":
                    Value = leftValue * rightValue;
                    Ccy = Left.Ccy;

                    // TODO ccy?
                    break;
                case "/":
                    Value = leftValue / rightValue;

                    // TODO ccy?
                    break;

                default:
                    Value = null;
                    FailedToParse = new[] {_symbol};
                    break;
            }
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