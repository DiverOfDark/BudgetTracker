using System;
using System.Collections.ObjectModel;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.JsModel
{
    [ExportJsModel]
    public class WidgetJsViewModel
    {
        private readonly WidgetModel _model;

        public WidgetJsViewModel(WidgetModel model)
        {
            _model = model;
        }

        public Guid Id => _model.Id;

        public string Kind => _model.Kind.GetDisplayName();

        public int KindId => (int) _model.Kind;

        public string Title => _model.Title;

        public ReadOnlyDictionary<string, string> Properties => _model.Properties;
    }
}