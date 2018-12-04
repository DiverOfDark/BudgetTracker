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
        private class PercentageCalculator
        {
            private double? _lastAdj;

            private readonly Stack<Stack<Tuple<DateTime, double, double>>> _history;
            private Stack<Tuple<DateTime, double, double>> _currentStack;
            
            public PercentageCalculator()
            {
                _history = new Stack<Stack<Tuple<DateTime, double, double>>>();
            }

            public void PushValue(DateTime when, double value, double adj)
            {
                if (adj != _lastAdj)
                {
                    _currentStack = new Stack<Tuple<DateTime, double, double>>();
                    _history.Push(_currentStack);
                }

                _currentStack.Push(Tuple.Create(when, value, adj));
                _lastAdj = adj;
            }

            public void Finalize(LastValueWidgetViewModel widget)
            {
                var weights = _history.Select(s=>s.OrderBy(t=>t.Item1).ToList()).Select(v => new
                {
                    delta = (v.Last().Item2 - v.First().Item2) / v.First().Item2,
                    count = (v.Last().Item1 - v.First().Item1).TotalDays,
                }).ToList();
                var totalCount = weights.Select(v => v.count).Sum();
                var yearDelta = weights.Select(v => v.delta * v.count / totalCount).Sum() * (365.0 / totalCount);

                var minimum = _history.First().First().Item2;
                var maximum = _history.Last().Last().Item2;
                
                (widget.ColorYear, widget.DeltaYear) = SetDiffPercenage(yearDelta);
                
                widget.Description = $"В начале: {widget.FormatValue(minimum)}\nВ конце: {widget.FormatValue(maximum)}\nГодовых: {yearDelta:P2}";
            }
        }
        
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

            var p = new PercentageCalculator();
            Values = new Dictionary<DateTime, double?>();
            bool first = true;
            foreach (var row in vm.Values.OrderByDescending(v => v.When).Where(v=> IsApplicable(v.When, period)))
            {
                var cell = row.Cells.GetValueOrDefault(column);

                var value = _settings.ExemptTransfers ? cell?.AdjustedValue : cell?.Value;
                
                if (value == null || double.IsNaN(value.Value))
                    continue;

                p.PushValue(row.When, cell.Value.Value, cell.Adjustment);
                
                Values[cell.Money?.When ?? row.When.Date] = value;
                IncompleteData |= cell.FailedToResolve.Any();

                if (first)
                {
                    first = false;

                    (Color, Delta) = SetDiffPercenage(cell.DiffPercentage);
                }
            }

            p.Finalize(this);

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