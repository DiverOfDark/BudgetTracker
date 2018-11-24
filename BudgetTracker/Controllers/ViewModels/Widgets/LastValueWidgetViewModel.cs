using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Model;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class LastValueWidgetViewModel : WidgetViewModel
    {
        private readonly LastValueWidgetSettings _settings;

        public LastValueWidgetViewModel(WidgetModel model, ObjectRepository repo, TableViewModelFactory vmf,
            int? period) : base(model, new LastValueWidgetSettings(model.Properties.ToDictionary(v=>v.Key,v=>v.Value)))
        {
            _settings = (LastValueWidgetSettings) Settings;
            var column = repo.Set<MoneyColumnMetadataModel>().First(v =>
                v.Provider == _settings.ProviderName &&
                (v.AccountName == _settings.AccountName || v.UserFriendlyName == _settings.AccountName));

            var vm = vmf.GetVM();
            vm.ShowAll = true;

            var tableRowViewModel = vm.Values.OrderByDescending(v => v.When).First(v=>v.Cells.ContainsKey(column));

            var matchedCell = tableRowViewModel.Cells.GetValueOrDefault(column);
            CurrentValue = matchedCell?.Value;
            CurrentDate = matchedCell?.Money?.When ?? tableRowViewModel.When.Date;
            
            Values = new Dictionary<DateTime, double?>();
            bool first = true;
            foreach (var row in vm.Values.OrderByDescending(v => v.When).Where(v=> IsApplicable(v.When, period)))
            {
                var cell = row.Cells.GetValueOrDefault(column);

                var value = _settings.ExemptTransfers ? cell?.AdjustedValue : cell?.Value;
                
                if (value == null || double.IsNaN(value.Value))
                    continue;

                Values[cell.Money?.When ?? row.When.Date] = value;
                IncompleteData |= cell.FailedToResolve.Any();

                if (first)
                {
                    first = false;

                    (Color, Delta) = SetDiffPercenage(cell.DiffPercentage);
                }
            }

            var adj = _settings.ExemptTransfers ? matchedCell?.AdjustedValue : matchedCell?.Value;
            
            var minValue = Values.OrderBy(v => v.Key).First();
            var maxValue = Values.OrderBy(v => v.Key).Last();

            adj = adj - maxValue.Value;
            
            var maximum = maxValue.Value + adj;
            var minimum = minValue.Value + adj;

            var dV = (maximum - minimum);
            var dt = (maxValue.Key - minValue.Key).TotalDays;

            var expPer = Math.Pow(maximum.GetValueOrDefault() / minimum.GetValueOrDefault(), 365/dt) - 1;

            var yearDelta = dV * 365.25 / minimum / dt;

            (ColorYear, DeltaYear) = SetDiffPercenage(yearDelta);
            Description = $"В начале: {FormatValue(minimum)}\nУчтено переводов: {FormatValue(adj)}\nВ конце: {FormatValue(maximum)}\nРазница: {FormatValue(dV)}\nСрок (дней): {dt}\nГодовых (простой процент): {yearDelta?.ToString("P2")}\nГодовых (с капитализацией): {expPer.ToString("P2")}";

            IncompleteData |= _settings.NotifyStaleData && Values.Select(v => v.Key).Max() < DateTime.Now.AddHours(-36);
        }

        public DateTime CurrentDate { get; set; }

        public double? CurrentValue { get; set; }

        private static (String color, String delta) SetDiffPercenage(double? cell)
        {
            string color, delta;
            if (cell != null)
            {
                cell = Math.Round(cell.Value, 4);
                
                color = cell > 0 ? "green" : "red";

                if (Math.Abs(cell.Value) <= 0.001)
                {
                    color = "yellow";
                }

                delta = (cell > 0 ? "+" : "") + cell.Value.ToString("P2");
            }
            else
            {
                color = "blue";
                delta = MiddleDash;
            }

            return (color, delta);
        }

        public string FormatDate(DateTime dt) => dt.ToLocalTime().TimeOfDay == TimeSpan.Zero ? dt.ToLocalTime().ToString("d") : dt.ToLocalTime().ToString("g");

        public string FormatValue(double? value)
        {
            if (value == null) return MiddleDash;

            if (!string.IsNullOrWhiteSpace(_settings.StringFormat))
                return string.Format(_settings.StringFormat, value);
            
            return value.ToString();
        }

        public override String TemplateName => WidgetExtensions.AsPath("~/Views/Widget/Widgets/LastValueWidget.cshtml");
        public override int Columns => IsCompact ? 2 : 4;

        public GraphKind GraphKind => _settings.GraphKind;
        public bool IsCompact => _settings.Compact;
        
        public bool IncompleteData { get; }
        
        public Dictionary<DateTime, double?> Values { get; }
        
        public string Delta { get; set; }
        public string DeltaYear { get; set; }
        public string Description { get; set; }

        public string Color { get; set; }
        public string ColorYear { get; set; }

        public string Provider => _settings.ProviderName;
        public string Account => _settings.AccountName;
    }
}