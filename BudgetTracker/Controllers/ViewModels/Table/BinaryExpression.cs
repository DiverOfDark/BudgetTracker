﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.JsModel;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class BinaryExpression : CalculateExpression
    {
        private readonly MoneyColumnMetadataJsModel _model;
        private readonly string _symbol;
        public static readonly string[] Symbols = { "/", "*", "+", "-", "??" }; 

        public BinaryExpression(MoneyColumnMetadataJsModel model, string symbol)
        {
            _model = model;
            _symbol = symbol;
        }

        public CalculateExpression Left { get; set; }
        public CalculateExpression Right { get; set; }

        public override void Evaluate(Dictionary<MoneyColumnMetadataJsModel, CalculatedResult> dependencies)
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


            switch (_symbol)
            {
                case "??":
                    value = leftValue ?? rightValue;
                    ccy = SelectCcy(Left.Value.Ccy, Right.Value.Ccy);
                    adjustment = leftValueAdj?.Adjustment ?? rightValueAdj?.Adjustment ?? 0; 
                    break;
                case "+":
                    value = (leftValue ?? 0) + (rightValue ?? 0);
                    ccy = SelectCcy(Left.Value.Ccy, Right.Value.Ccy);
                    adjustment = (leftValueAdj?.Adjustment ?? 0) + (rightValueAdj?.Adjustment ?? 0);
                    break;
                case "-":
                    value = (leftValue ?? 0) - (rightValue ?? 0);
                    ccy = SelectCcy(Left.Value.Ccy, Right.Value.Ccy);
                    adjustment = (leftValueAdj?.Adjustment ?? 0) - (rightValueAdj?.Adjustment ?? 0);
                    break;
                
                case "*":
                    value = (leftValue ?? 0) * (rightValue ?? 0);
                    adjustment = (leftValueAdj?.Adjustment ?? 0) * (rightValueAdj?.Adjustment ?? 0);
                    ccy = SelectCcy(Left.Value.Ccy, Right.Value.Ccy);

                    // TODO ccy?
                    break;
                case "/":
                    value = (leftValue ?? 0) / (rightValue ?? 0);

                    var leftAdj = leftValueAdj?.Adjustment ?? 0;
                    var rightAdj = rightValueAdj?.Adjustment ?? 0;

                    if (Math.Abs(rightAdj) < double.Epsilon)
                    {   
                        rightAdj = 1;
                    }
                    
                    adjustment = leftAdj / rightAdj;
                    ccy = SelectCcy(Left.Value.Ccy, Right.Value.Ccy);

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