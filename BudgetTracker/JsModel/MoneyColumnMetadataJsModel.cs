using System;
using System.Linq;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.JsModel
{
    [ExportJsModel]
    public class MoneyColumnMetadataJsModel
    {
        private readonly ObjectRepository _repository;
        private readonly MoneyColumnMetadataModel _model;

        public  MoneyColumnMetadataJsModel(ObjectRepository repository, MoneyColumnMetadataModel model)
        {
            _repository = repository;
            _model = model;
        }

        public string AccountName => _model.AccountName;

        public string Function => _model.Function;

        public string Provider => _model.Provider;

        public string UserFriendlyName => _model.UserFriendlyName;

        public bool IsComputed => _model.IsComputed;

        public bool AutogenerateStatements => _model.AutogenerateStatements;
 
        public bool CanDelete => _model.IsComputed || _repository.Set<MoneyStateModel>()
                                     .All(s => s.AccountName != _model.AccountName && s.Provider != _model.Provider)
                                 && _repository.Set<PaymentModel>().All(s => s.Column != _model);

        public Guid Id => _model.Id;
    }
}