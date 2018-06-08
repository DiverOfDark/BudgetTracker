using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class LastValueWidgetViewModel : WidgetViewModel
    {
        private readonly LastValueWidgetSettings _settings;

        public LastValueWidgetViewModel(WidgetModel model, ObjectRepository repo, TableViewModel vm) : base(model, new LastValueWidgetSettings(model.Properties.ToDictionary(v=>v.Key,v=>v.Value)))
        {
            _settings = (LastValueWidgetSettings) Settings;
            var column = repo.Set<MoneyColumnMetadataModel>().First(v =>
                v.Provider == _settings.ProviderName &&
                (v.AccountName == _settings.AccountName || v.UserFriendlyName == _settings.AccountName));

            vm = new TableViewModel(vm)
            {
                ShowAll = true
            };
            
            Values = new Dictionary<DateTime, double?>();
            bool first = true;
            foreach (var row in vm.Values.OrderByDescending(v => v.When))
            {
                var cell = row.Cells.FirstOrDefault(v => v.Column == column);
                if (cell == null)
                    continue;
                
                Values[cell.Money?.When ?? row.When.Date] = cell.Value;
                IncompleteData |= !cell.Value.HasValue || cell.FailedToResolve.Any();

                if (first)
                {
                    first = false;

                    (Color, Delta) = SetDiffPercenage(cell.DiffPercentage);
                }
            }

            var minValue = Values.OrderBy(v => v.Key).First();
            var maxValue = Values.OrderBy(v => v.Key).Last();

            var dV = (maxValue.Value - minValue.Value) / minValue.Value;
            var dt = (maxValue.Key - minValue.Key).TotalDays;

            var yearDelta = dV / dt * 365.25;

            (ColorYear, DeltaYear) = SetDiffPercenage(yearDelta);

            IncompleteData |= _settings.NotifyStaleData && Values.Select(v => v.Key).Max() < DateTime.Now.AddDays(-1);
        }

        private static (String color, String delta) SetDiffPercenage(double? cell)
        {
            string color, delta;
            if (cell != null)
            {
                cell = Math.Round(cell.Value, 2);
                
                color = cell > 0 ? "green" : "red";

                if (Math.Abs(cell.Value) <= 0.01)
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

        public string Color { get; set; }
        public string ColorYear { get; set; }
    }
}