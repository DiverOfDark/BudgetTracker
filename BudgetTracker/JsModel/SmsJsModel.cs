using System;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.JsModel
{
    [ExportJsModel]
    public class SmsJsModel
    {
        private readonly SmsModel _smsModel;

        public SmsJsModel(SmsModel smsModel)
        {
            _smsModel = smsModel;
        }

        public DateTime When => _smsModel.When;
        public string From => _smsModel.From;
        public string Message => _smsModel.Message;
        public Guid Id => _smsModel.Id;

        public bool IsHidden => _smsModel.AppliedRule != null;
    }
}