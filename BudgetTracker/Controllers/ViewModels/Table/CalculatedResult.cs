using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetTracker.Model;
using Microsoft.EntityFrameworkCore.Internal;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class CalculatedResult
    {
        private double? _value;
        private string _tooltip;
        private string _ccy;
        private IEnumerable<string> _failedToResolve;
        private double _adjustment;

        public static CalculatedResult FromComputed(Dictionary<string, MoneyColumnMetadataModel> columns, MoneyColumnMetadataModel h, Dictionary<MoneyColumnMetadataModel, CalculatedResult> deps)
        {
            return new ExpressionCalculatedResult(columns, h, deps);
        }

        public static CalculatedResult FromMoney(MoneyColumnMetadataModel h, MoneyStateModel money, double adjustment) => new CalculatedResult(h)
        {
            Money = money,
            _ccy = money.Ccy,
            _adjustment = adjustment,
            _value = money.Amount,
            _tooltip = $"{(money.Amount + adjustment).ToString(CultureInfo.CurrentCulture)}({money.Amount.ToString(CultureInfo.CurrentCulture)} + {adjustment.ToString(CultureInfo.CurrentCulture)})"
        };

        public static CalculatedResult Empty(MoneyColumnMetadataModel item) => new CalculatedResult(item)
        {
            _ccy = null,
            _value = double.NaN
        };

        public static CalculatedResult ResolutionFail(MoneyColumnMetadataModel item, params string[] failedToResolve) => new CalculatedResult(item)
        {
            _ccy = null,
            _value = double.NaN,
            _failedToResolve = failedToResolve
        };

        public static CalculatedResult FromComputed(MoneyColumnMetadataModel item, double? value, string ccy,
            IEnumerable<string> failedToResolve, double adjustment, string tooltip) => new CalculatedResult(item)
        {
            _value = value,
            _ccy = ccy,
            _failedToResolve = failedToResolve,
            _adjustment = adjustment,
            _tooltip = tooltip
        };
        
        protected CalculatedResult(MoneyColumnMetadataModel item)
        {
            Column = item;
            _failedToResolve = Enumerable.Empty<string>();
        }

        public MoneyColumnMetadataModel Column { get; }

        public MoneyStateModel Money { get; private set;}

        public virtual double Adjustment => _adjustment;
        
        public virtual double? Value => _value;

        public double? AdjustedValue => Value + Adjustment;

        public virtual IEnumerable<string> FailedToResolve => _failedToResolve;

        public virtual string Tooltip => _tooltip;

        public virtual string Ccy => _ccy;

        public CalculatedResult PreviousValue { get; set; }

        public double? DiffValue => AdjustedValue - PreviousValue?.AdjustedValue;

        public double? DiffPercentage => DiffValue / PreviousValue?.AdjustedValue;
        
        public override string ToString() => $"[{Column.Provider}/{Column.AccountName}]({AdjustedValue})";
    }
}