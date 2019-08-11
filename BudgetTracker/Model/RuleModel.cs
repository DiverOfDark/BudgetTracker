using System;
using System.Collections.Generic;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Model
{
    public sealed class RuleModel : ModelBase
    {
        public class RuleEntity : BaseEntity
        {
            public int RuleType { get; set; }
            public string RegexSender { get; set; }
            public string RegexText { get; set; }
        }

        private readonly RuleEntity _entity;

        public RuleModel(RuleEntity entity)
        {
            _entity = entity;
        }

        public RuleModel(RuleType ruleType, string regexSender, string regexText)
        {
            _entity = new RuleEntity
            {
                Id = Guid.NewGuid(),
                RuleType = (int)ruleType,
                RegexSender = regexSender,
                RegexText = regexText
            };
        }

        protected override BaseEntity Entity => _entity;

        public RuleType RuleType => (RuleType) _entity.RuleType;
        public string RegexSender => _entity.RegexSender;
        public string RegexText => _entity.RegexText;
        
        public IEnumerable<SmsModel> Smses => Multiple<SmsModel>(() => x => x.AppliedRuleId);
    }
}