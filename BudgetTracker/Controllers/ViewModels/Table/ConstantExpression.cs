using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class ConstantExpression : CalculateExpression
    {
        private readonly MoneyColumnMetadataModel _model;
        private readonly string _source;
        private static Regex parseRegex = new Regex(@"(?<value>[0-9.]+)(\:(?<adj>[0-9.]+))?(?<ccy>\@.+)?", RegexOptions.Compiled);
        
        public ConstantExpression(MoneyColumnMetadataModel model, string source)
        {
            _model = model;
            _source = source;
        }
        
        public override void Evaluate(Dictionary<MoneyColumnMetadataModel, CalculatedResult> dependencies)
        {
            var match = parseRegex.Match(_source);

            var valueString = match.Groups["value"].Captures.FirstOrDefault()?.Value;
            var ccy = match.Groups["ccy"].Captures.FirstOrDefault()?.Value;
            var adjustmentString = match.Groups["adj"].Captures.FirstOrDefault()?.Value;

            double.TryParse(valueString, out var value);
            double.TryParse(adjustmentString, out var adjustment);
            
            Value = CalculatedResult.FromComputed(_model, value, ccy, Enumerable.Empty<string>(), adjustment, ToString());
        }

        public override CalculateExpression TryApply(CalculateExpression otherExpression)
        {
            throw new NotImplementedException();
        }

        protected override string ToStringImpl() => "{" + _source + "}";
    }
}