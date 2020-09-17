using System;
using System.Collections.Generic;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.JsModel
{
    [ExportJsModel]
    public class MoneyColumnMetadataJsModel
    {
        private readonly MoneyColumnMetadataModel _model;

        internal MoneyColumnMetadataModel Column => _model;
        
        public  MoneyColumnMetadataJsModel(MoneyColumnMetadataModel model)
        {
            _model = model;
        }

        public string AccountName => _model.AccountName;

        public string Function => _model.Function;

        public string Provider => _model.Provider;

        public string UserFriendlyName => _model.UserFriendlyName;

        public bool IsComputed => _model.IsComputed;

        public IEnumerable<String> ChartList => _model.ChartList;

        public bool AutogenerateStatements => _model.AutogenerateStatements;
 
        public Guid Id => _model.Id;
    }
}