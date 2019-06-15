using System;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.JsModel
{
    [ExportJsModel]
    public class SpentCategoryJsModel
    {
        private readonly SpentCategoryModel _model;

        public SpentCategoryJsModel(SpentCategoryModel model)
        {
            _model = model;
        }

        public string Category => _model.Category;
        
        public string Pattern => _model.Pattern;

        public string Kind => _model.Kind.GetDisplayName();

        public int KindId => (int) _model.Kind;

        public Guid Id => _model.Id;
    }
}