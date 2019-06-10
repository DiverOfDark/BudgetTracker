using System;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.JsModel
{
    [ExportJsModel]
    public class SmsRuleJsModel
    {
        private readonly RuleModel _ruleModel;

        public SmsRuleJsModel(RuleModel ruleModel)
        {
            _ruleModel = ruleModel;
        }

        public Guid Id => _ruleModel.Id;

        public string Sender => _ruleModel.RegexSender;
        public string Text => _ruleModel.RegexText;
        public string RuleType => _ruleModel.RuleType.GetDisplayName();
    }
}