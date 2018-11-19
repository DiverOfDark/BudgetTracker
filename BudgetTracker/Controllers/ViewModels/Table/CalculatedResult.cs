using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetTracker.Model;
using Microsoft.EntityFrameworkCore.Internal;

namespace BudgetTracker.Controllers.ViewModels.Table
{
    public class CalculatedResult
    {
        public static CalculatedResult FromMoney(MoneyColumnMetadataModel h, MoneyStateModel money, double adjustment) => new CalculatedResult
        {
            Ccy = money.Ccy,
            Column = h,
            Money = money,
            Value = money.Amount + adjustment,
            Tooltip = $"{(money.Amount + adjustment).ToString(CultureInfo.CurrentCulture)}({money.Amount.ToString(CultureInfo.CurrentCulture)} + {adjustment.ToString(CultureInfo.CurrentCulture)})"
        };

        public static CalculatedResult Empty(MoneyColumnMetadataModel item) => new CalculatedResult
        {
            Ccy = null,
            Column = item,
            Value = double.NaN
        };
        
        protected CalculatedResult() { }

        public MoneyColumnMetadataModel Column { get; private set; }

        public MoneyStateModel Money { get; private set;}

        public double? Value { get; set; }

        public IEnumerable<string> FailedToResolve { get; set; } = Enumerable.Empty<string>();

        public string Tooltip { get; set; }

        public string Ccy { get; set; }

        public CalculatedResult PreviousValue { get; set; }

        public double? DiffValue => Value - PreviousValue?.Value;

        public double? DiffPercentage => DiffValue / PreviousValue?.Value;
        
        public override string ToString()
        {
            return $"[{Column.Provider}/{Column.AccountName}]({Value})";
        }
    }
}