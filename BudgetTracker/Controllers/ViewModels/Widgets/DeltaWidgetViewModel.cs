using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class DeltaWidgetViewModel : WidgetViewModel
    {
        public DeltaWidgetViewModel(WidgetModel model, ObjectRepository repository, TableViewModel vm) : base(model,
            new DeltaWidgetSettings(model.Properties.ToDictionary(v => v.Key, v => v.Value)))
        {
            DeltaWidgetSettings = (DeltaWidgetSettings) Settings;

            var table = new TableViewModel(vm);

            var col = repository.Set<MoneyColumnMetadataModel>().First(v =>
                v.Provider == DeltaWidgetSettings.ProviderName &&
                (v.AccountName == DeltaWidgetSettings.AccountName ||
                 v.UserFriendlyName == DeltaWidgetSettings.AccountName));

            if (DeltaWidgetSettings.DeltaInterval == DeltaInterval.Daily)
            {
                AddDelta(col, table, 1, "24ч");
                AddDelta(col, table, 2, "48ч");
                AddDelta(col, table, 7, "1н");
                AddDelta(col, table, 30, "1м");
            }
            else
            {
                AddDelta(col, table, 30, "1м");
                AddDelta(col, table, 90, "3м");
                AddDelta(col, table, 182, "6м");
                AddDelta(col, table, 365, "1г");
                
            }
        }

        private void AddDelta(MoneyColumnMetadataModel col, TableViewModel table, int daysDiff, string name)
        {
            var sequence = table.Values.SkipWhile(v=>
            {
                var s = v.Cells.GetValueOrDefault(col)?.Value;
                return s == null || double.IsNaN(s.Value);
            }).OrderByDescending(v => v.When);
            var today = sequence.FirstOrDefault();
            var baseSet = sequence.FirstOrDefault(v => v.When.Date.AddDays(daysDiff) <= DateTime.Now.Date);

            var todayValue = today?.Cells.GetValueOrDefault(col);
            var baseSetValue = baseSet?.Cells.GetValueOrDefault(col);

            if (todayValue != null && baseSetValue != null)
            {
                Ccy = Ccy ?? todayValue.Ccy;

                IncompleteData |= todayValue.FailedToResolve.Concat(baseSetValue.FailedToResolve).Any();
                
                var dT = todayValue.AdjustedValue - baseSetValue.AdjustedValue;

                if (dT != null && !double.IsNaN(dT.Value) && !double.IsInfinity(dT.Value))
                {
                    Deltas.Add((name, dT.Value));
                }
                else
                {
                    IncompleteData = true;
                }
            }
        }

        public DeltaWidgetSettings DeltaWidgetSettings { get; set; }
        public override string TemplateName => WidgetExtensions.AsPath("~/Views/Widget/Widgets/DeltaWidget.cshtml");
        public override int Columns => 4;
        public bool IncompleteData { get; private set; }
        
        public string Ccy { get; private set; }
        
        public List<(string, double)> Deltas { get; } = new List<(string, double)>(); 
    }
}