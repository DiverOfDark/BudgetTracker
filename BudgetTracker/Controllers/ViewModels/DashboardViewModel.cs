﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Controllers.ViewModels.Widgets;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels
{
    [ExportJsModel]
    public class DashboardViewModel
    {
        private readonly ObjectRepository _objectRepository;
        private readonly TableViewModelFactory _vm;

        public DashboardViewModel(ObjectRepository objectRepository, int? period, TableViewModelFactory vm)
        {
            _objectRepository = objectRepository;
            _vm = vm;
            Period = period;
            
            var widgetViewModels = CreateWidgetViewModels();

            Widgets = GroupRows(widgetViewModels);
        }

        private List<BootstrapColumnViewModel> GroupRows(List<WidgetViewModel> widgetViewModels)
        {
            var result = new List<BootstrapColumnViewModel>();

            int columnCount = 0;
            int rowCount = 0;
            int finishedCells = 0;
            
            var source = new Queue<WidgetViewModel>(widgetViewModels);
            
            while(source.TryPeek(out var widget))
            {
                if (columnCount + widget.Columns <= 12)
                {
                    var cell = new List<WidgetViewModel> {widget};

                    result.Add(new BootstrapColumnViewModel
                    {
                        Columns = widget.Columns,
                        Rows = cell
                    });

                    columnCount += widget.Columns;
                    rowCount = Math.Max(rowCount, widget.Rows);

                    source.Dequeue();
                }
                else
                {
                    var existingColumnToAdd = result.Skip(finishedCells).FirstOrDefault(v =>
                        v.Columns == widget.Columns && v.Rows.Sum(s => s.Rows) + widget.Rows <= rowCount);

                    if (existingColumnToAdd == null)
                    {
                        columnCount = 0;
                        rowCount = 0;
                        finishedCells = result.Count;
                    }
                    else
                    {
                        existingColumnToAdd.Rows.Add(widget);
                        source.Dequeue();
                    }
                }
            }

            return result;
        }

        private List<WidgetViewModel> CreateWidgetViewModels()
        {
            var widgetViewModels = new List<WidgetViewModel>();
            var widgets = _objectRepository.Set<WidgetModel>().OrderBy(v => v.Order).ToList();
            foreach (var w in widgets)
            {
                try
                {
                    switch (w)
                    {
                        case var wi when w.Properties.Count(s => !string.IsNullOrWhiteSpace(s.Value)) == 0:
                            widgetViewModels.Add(new UnconfiguredWidgetViewModel(wi));
                            break;
                        case var wi when w.Kind == WidgetKind.LastValue:
                            widgetViewModels.Add(new LastValueWidgetViewModel(wi, _vm, Period));
                            break;
                        case var wi when w.Kind == WidgetKind.Expenses:
                            widgetViewModels.Add(new ExpensesWidgetViewModel(wi, _objectRepository, Period));
                            break;
                        case var wi when w.Kind == WidgetKind.Donut:
                            widgetViewModels.Add(new DonutWidgetViewModel(wi, Period, _vm.GetVM()));
                            break;
                        case var wi when w.Kind == WidgetKind.LinearChart:
                            widgetViewModels.Add(new LinearChartWidgetViewModel(wi, Period, _vm.GetVM()));
                            break;
                        case var wi when w.Kind == WidgetKind.Delta:
                            widgetViewModels.Add(new DeltaWidgetViewModel(wi, _vm.GetVM()));
                            break;
                        default:
                            widgetViewModels.Add(new UnknownWidgetViewModel(w));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    widgetViewModels.Add(new ExceptionWidgetViewModel(w, ex));
                }
            }

            return widgetViewModels;
        }

        public List<BootstrapColumnViewModel> Widgets { get; }
        public bool ShowButtons { get; set; }
        public int? Period { get; set; }

        public string PeriodFriendly
        {
            get
            {
                if (Period == 0 || Period == null)
                    return "всё время";

                if (Period == 1)
                    return "1 месяц";
                
                if (Period == 3)
                    return "3 месяца";
                if (Period == 6)
                    return "6 месяцев";

                return Period + " месяца(ев)";
            }
        }
    }

    [ExportJsModel]
    public class BootstrapColumnViewModel
    {
        public int Columns { get; set; }
        
        public List<WidgetViewModel> Rows { get; set; }
    }
}