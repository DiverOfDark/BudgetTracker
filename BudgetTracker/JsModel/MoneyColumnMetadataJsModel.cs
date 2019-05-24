using BudgetTracker.Model;

namespace BudgetTracker.JsModel
{
    [ExportJsModel]
    public class MoneyColumnMetadataJsModel
    {
        private readonly MoneyColumnMetadataModel _model;

        public  MoneyColumnMetadataJsModel(MoneyColumnMetadataModel model)
        {
            _model = model;
        }

        public string AccountName => _model.AccountName;
    }
}