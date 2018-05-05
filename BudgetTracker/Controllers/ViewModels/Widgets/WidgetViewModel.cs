using System;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public abstract class WidgetViewModel
    {
        protected const string MiddleDash = "-";
        
        private readonly WidgetModel _model;

        public WidgetViewModel(WidgetModel model, WidgetSettings settings)
        {
            Settings = settings;
            _model = model;
        }
        
        protected WidgetSettings Settings { get; }

        public abstract string TemplateName { get; }

        public string Title => _model.Title;
        public Guid Id => _model.Id;
        public abstract int Columns {get;}
        public virtual int Rows => 1;
        
        public int Order => _model.Order;
    }
}