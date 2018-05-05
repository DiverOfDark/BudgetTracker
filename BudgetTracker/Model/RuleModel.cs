using System;
using System.Collections.Generic;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;

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
            Id = Guid.Parse(_entity.RowKey);
        }

        public RuleModel(RuleType ruleType, string regexSender, string regexText)
        {
            Id = Guid.NewGuid();
            _entity = new RuleEntity
            {
                PartitionKey = nameof(RuleModel),
                RowKey = Id.ToString(),
                RuleType = (int)ruleType,
                RegexSender = regexSender,
                RegexText = regexText
            };
        }

        public override Guid Id { get; }
        protected override object Entity => _entity;

        public RuleType RuleType => (RuleType) _entity.RuleType;
        public string RegexSender => _entity.RegexSender;
        public string RegexText => _entity.RegexText;
        
        public IEnumerable<SmsModel> Smses => Multiple<SmsModel>(x => x.AppliedRuleId);
    }
}