﻿using System;
using System.Collections.Generic;
using BudgetTracker.JsModel;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class ReferenceExpression : CalculateExpression
    {
        private readonly MoneyColumnMetadataJsModel _column;

        private bool _evaluated;
        
        public ReferenceExpression(MoneyColumnMetadataJsModel column)
        {
            _column = column;
        }

        public override void Evaluate(Dictionary<MoneyColumnMetadataJsModel, CalculatedResult> dependencies)
        {
            if (!_evaluated)
            {
                _evaluated = true;

                dependencies.TryGetValue(_column, out var matchedDependency);

                Value = matchedDependency?.IsOk == true
                    ? matchedDependency
                    : CalculatedResult.ResolutionFail(_column,
                        _column.IsComputed
                            ? _column.UserFriendlyName
                            : (_column.Provider + "/" + (_column.UserFriendlyName ?? _column.AccountName)));
            }
        }
        
        public override CalculateExpression TryApply(CalculateExpression otherExpression) => throw new NotSupportedException();

        protected override string ToStringImpl() => $"[{_column.Provider}/{_column.UserFriendlyName ?? _column.AccountName}]({Value})";
    }
}