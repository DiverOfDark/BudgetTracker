using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Model
{
    public sealed class SmsModel : ModelBase
    {
        public class SmsEntity : BaseEntity
        {
            public string From { get; set; }
            public string Message { get; set; }
            public DateTime When { get; set; }
            public Guid? AppliedRule { get; [UsedImplicitly] set; }
        }

        private readonly SmsEntity _entity;

        public SmsModel(SmsEntity entity)
        {
            _entity = entity;
        }

        public SmsModel(string from, string message, DateTime when)
        {
            _entity = new SmsEntity
            {
                Id = Guid.NewGuid(),
                From = from,
                Message = message,
                When = when
            };
        }

        protected override BaseEntity Entity => _entity;

        public Guid? AppliedRuleId => _entity.AppliedRule;

        public RuleModel AppliedRule
        {
            get => Single<RuleModel>(_entity.AppliedRule);
            set => UpdateProperty(_entity, () => x => x.AppliedRule, value?.Id);
        }

        public string From
        {
            get => _entity.From;
            set => UpdateProperty(_entity, () => x => x.From, value);
        }

        public string Message
        {
            get => _entity.Message;
            set => UpdateProperty(_entity, () => x => x.Message, value);
        }

        public DateTime When => _entity.When;
        
        public IEnumerable<PaymentModel> Payments => Multiple<PaymentModel>(() => v => v.SmsId);
    }
}