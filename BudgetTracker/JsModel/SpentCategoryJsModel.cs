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

        public string Kind => _model.Kind.ToString();

        public Guid Id => _model.Id;
    }
}