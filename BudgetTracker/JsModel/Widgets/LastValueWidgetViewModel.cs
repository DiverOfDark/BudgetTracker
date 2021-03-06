﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    [ExportJsModel]
    public class LastValueWidgetViewModel : WidgetViewModel
    {
        private class PercentageCalculator
        {
            private readonly Stack<Tuple<DateTime, double, double>> _history;
            
            public PercentageCalculator()
            {
                _history = new Stack<Tuple<DateTime, double, double>>();
            }

            public void PushValue(DateTime when, double value, double adj)
            {
                _history.Push(Tuple.Create(when, value, adj));
            }

            public void Finalize(LastValueWidgetViewModel widget)
            {
                Tuple<DateTime, double, double> current = null;

                var percentages = new Stack<double>();

                double minimum = 0, maximum = 0;
                
                while (_history.TryPop(out var next))
                {
                    if (current != null)
                    {
                        var nextValue = next.Item2;
                        var currentValue = current.Item2;

                        currentValue -= next.Item3 - current.Item3;

                        var percentage = nextValue / currentValue;
                        percentages.Push(percentage);
                        current = next;
                        maximum = current.Item2;
                    }
                    else
                    {
                        current = next;
                        minimum = current.Item2;
                    }
                }

                double yearDelta = Double.NaN;

                if (percentages.Any())
                {
                    var avg = Math.Pow(percentages.Aggregate((a, b) => a * b), 1.0 / percentages.Count) - 1;
                    // var avg = percentages.Average() - 1;

                    yearDelta = avg * 365.25;
                }
                

                (widget.ColorYear, widget.DeltaYear) = SetDiffPercenage(yearDelta);
                
                widget.Description = $"В начале: {widget.FormatValue(minimum)}\nВ конце: {widget.FormatValue(maximum)}\nГодовых: {yearDelta:P2}";
            }
        }
        
        protected const string MiddleDash = "—";

        private readonly LastValueWidgetSettings _settings;

        public LastValueWidgetViewModel(WidgetModel model, TableViewModelFactory vmf,
            int? period) : base(model, new LastValueWidgetSettings(model.Properties.ToDictionary(v=>v.Key,v=>v.Value)))
        {
            _settings = (LastValueWidgetSettings) Settings;
            var vm = vmf.GetVM();

            var column = vm.Headers.First(v =>
                v.Provider == _settings.ProviderName &&
                (v.AccountName == _settings.AccountName || v.UserFriendlyName == _settings.AccountName));


            var tableRowViewModel = vm.Values.OrderByDescending(v => v.When).FirstOrDefault(v=>v.CalculatedCells.GetValueOrDefault(column)?.IsOk == true);

            var matchedCell = tableRowViewModel?.CalculatedCells.GetValueOrDefault(column);
            CurrentValue = matchedCell?.Value;
            CurrentDate = matchedCell?.Money?.When ?? tableRowViewModel?.When.Date ?? DateTime.MinValue;

            var p = new PercentageCalculator();
            Values = new Dictionary<DateTime, double?>();
            bool first = true;
            foreach (var row in vm.Values.OrderByDescending(v => v.When).Where(v=> IsApplicable(v.When, period)))
            {
                var cell = row.CalculatedCells.GetValueOrDefault(column);

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

            IncompleteData |= _settings.NotifyStaleData && (!Values.Any() || Values.Select(v => v.Key).Max() < DateTime.Now.AddHours(-36));

            if (IsCompact)
            {
                Values = null;
            }
        }

        public bool ExemptTransfers => _settings.ExemptTransfers;
        
        public DateTime CurrentDate { get; set; }

        public double? CurrentValue { get; set; }

        public string CurrentValueFormatted => FormatValue(CurrentValue);

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

        public string FormatValue(double? value)
        {
            if (value == null) return MiddleDash;

            if (!string.IsNullOrWhiteSpace(_settings.StringFormat))
                return string.Format(_settings.StringFormat, value);
            
            return value.ToString();
        }

        public override int Columns => IsCompact ? 2 : 4;

        public int GraphKind => (int) _settings.GraphKind;
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