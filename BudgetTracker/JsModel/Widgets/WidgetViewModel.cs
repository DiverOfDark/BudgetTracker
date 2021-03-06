﻿using System;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;
using JetBrains.Annotations;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    [ExportJsModel]
    public abstract class WidgetViewModel
    {
        public WidgetViewModel(WidgetModel model, WidgetSettings settings)
        {
            Settings = settings;
            Title = model?.Title;
            Id = model?.Id;
        }
        
        protected bool IsApplicable(DateTime argWhen, int? period)
        {
            if (period == null || period == 0)
                return true;

            return argWhen.AddMonths(period.Value) > DateTime.Now;
        }

        public WidgetSettings Settings { get; }

        [UsedImplicitly]
        public string Kind => GetType().Name;
        public string Title { get; protected set; }
        public Guid? Id {get; }
        public abstract int Columns {get;}
        public virtual int Rows => 1;
    }
}