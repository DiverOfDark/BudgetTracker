using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BudgetTracker.JsModel;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class ConstantExpression : CalculateExpression
    {
        private readonly MoneyColumnMetadataJsModel _model;
        private readonly string _source;
        private static Regex parseRegex = new Regex(@"(?<value>[0-9.,]+)(\:(?<adj>[0-9.,]+))?(?<ccy>\@.+)?", RegexOptions.Compiled);
        
        public ConstantExpression(MoneyColumnMetadataJsModel model, string source)
        {
            _model = model;
            _source = source;
        }
        
        public override void Evaluate(Dictionary<MoneyColumnMetadataJsModel, CalculatedResult> dependencies)
        {
            var match = parseRegex.Match(_source);

            var valueString = match.Groups["value"].Captures.FirstOrDefault()?.Value;
            var ccy = match.Groups["ccy"].Captures.FirstOrDefault()?.Value;
            var adjustmentString = match.Groups["adj"].Captures.FirstOrDefault()?.Value;

            valueString = valueString?.Replace(",", ".");
            adjustmentString = adjustmentString?.Replace(",", ".");

            var numberFormatInfo = new NumberFormatInfo{NumberDecimalSeparator = "."};
            double.TryParse(valueString, NumberStyles.Any, numberFormatInfo, out var value);
            double.TryParse(adjustmentString, NumberStyles.Any, numberFormatInfo, out var adjustment);
            
            Value = CalculatedResult.FromComputed(_model, value, ccy, Enumerable.Empty<string>(), adjustment, ToString());
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression)
        {
            throw new NotImplementedException();
        }

        protected override string ToStringImpl() => "{" + _source + "}";
    }
}