using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class BinaryExpression : CalculateExpression
    {
        private readonly MoneyColumnMetadataModel _model;
        private readonly string _symbol;
        public static readonly string[] Symbols = { "/", "*", "+", "-", "??" }; 

        public BinaryExpression(MoneyColumnMetadataModel model, string symbol)
        {
            _model = model;
            _symbol = symbol;
        }

        public CalculateExpression Left { get; set; }
        public CalculateExpression Right { get; set; }

        public override void Evaluate(Dictionary<MoneyColumnMetadataModel, CalculatedResult> dependencies)
        {
            if (Left == null || Right == null)
            {
                Value = CalculatedResult.ResolutionFail(_model, _symbol);
                
                return;
            }

            Left.Evaluate(dependencies);
            Right.Evaluate(dependencies);

            var leftValue = Left.Value.Value;
            var rightValue = Right.Value.Value;

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
            
            IEnumerable<string> failedToResolve = Left.Value.FailedToResolve.Concat(Right.Value.FailedToResolve).ToList();
            double? value = null;
            double adjustment = 0;
            string ccy = null;
            
            var leftValueAdj = Left.Value;
            var rightValueAdj = Right.Value;
                    
            while (leftValueAdj?.Value != null && double.IsNaN(leftValueAdj.Value.Value))
            {
                leftValueAdj = leftValueAdj.PreviousValue;
            }
            while (rightValueAdj?.Value != null && double.IsNaN(rightValueAdj.Value.Value))
            {
                rightValueAdj = rightValueAdj.PreviousValue;
            }

            adjustment = (leftValueAdj?.Adjustment ?? 0) + (rightValueAdj?.Adjustment ?? 0);

            switch (_symbol)
            {
                case "??":
                    value = leftIsNan ? rightValue : leftValue;
                    ccy = SelectCcy(Left.Value.Ccy, Right.Value.Ccy);
                    failedToResolve = leftIsNan ? Right.Value.FailedToResolve : Left.Value.FailedToResolve;

                    break;
                case "+":
                    value = leftValue + rightValue;
                    ccy = SelectCcy(Left.Value.Ccy, Right.Value.Ccy);
                    break;
                case "-":
                    value = leftValue - rightValue;
                    ccy = SelectCcy(Left.Value.Ccy, Right.Value.Ccy);
                    break;
                
                case "*":
                    value = leftValue * rightValue;
                    ccy = Left.Value.Ccy;

                    // TODO ccy?
                    break;
                case "/":
                    value = leftValue / rightValue;

                    // TODO ccy?
                    break;

                default:
                    failedToResolve = new[] {_symbol};
                    break;
            }

            Value = CalculatedResult.FromComputed(_model, value, ccy, failedToResolve, adjustment, ToString());
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
        
        protected override string ToStringImpl() => $"{Left} {_symbol} {Right}";
    }
}