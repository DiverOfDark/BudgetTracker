using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;

namespace BudgetTracker.Model
{
    public enum PaymentKind
    {
        Expense = 0,
        Income = 1,
        Transfer = 2,
        Unknown = -1
    }
    
    public class PaymentModel : ModelBase
    {
        public class PaymentEntity : BaseEntity
        {
            public string What { get; set; }
            public DateTime When { get; set; }
            public double Amount { get; set; }
            public string Ccy { get; set; }
            public Guid? SmsId { get; set; }
            public Guid? CategoryId { get; set; }
            
            public string Provider { get; set; }
            public string Account { get; set; }
            public int Kind { get; set; }
            public string StatementReference { get; set; }
        }
        
        private readonly PaymentEntity _entity;

        public PaymentModel(PaymentEntity entity)
        {
            _entity = entity;
            Id = Guid.Parse(_entity.RowKey);
        }

        public PaymentModel(string providerName, string account, DateTime when, string what, double amount, string ccy, string statementReference)
        {
            Id = Guid.NewGuid();
            _entity = new PaymentEntity
            {
                PartitionKey = nameof(MoneyStateModel),
                RowKey = Id.ToString(),
                Provider = providerName,
                Account = account,
                When = when,
                Ccy = ccy,
                Amount = amount,
                What = what,
                Kind = (int) (amount < 0 ? PaymentKind.Expense : PaymentKind.Income),
                StatementReference = statementReference
            };
        }

        public PaymentModel(SmsModel sms, RuleModel rule)
        {
            var matches = Regex.Match(sms.Message, rule.RegexText);

            var amount = matches.Groups.First(v => v.Name == "sum").Value;

            amount = new string(amount.Replace(",", ".").Where(v=>char.IsDigit(v) || v == '.').ToArray());

            Id = Guid.NewGuid();
            _entity = new PaymentEntity
            {
                PartitionKey = nameof(MoneyStateModel),
                RowKey = Id.ToString(),
                SmsId = sms.Id,
                When = sms.When,
                Amount = double.Parse(amount, new NumberFormatInfo
                {
                    NumberDecimalSeparator = "."
                }),
                What = matches.Groups.First(v => v.Name == "what").Value,
                Ccy = matches.Groups.First(v => v.Name == "ccy").Value,
                Kind = (int) PaymentKind.Expense
            };
        }

        public sealed override Guid Id { get; }
        protected override object Entity => _entity;

        public Guid? CategoryId => _entity.CategoryId;
        public Guid? SmsId => _entity.SmsId;
        public DateTime When => _entity.When;

        public string What
        {
            get => _entity.What;
            set => UpdateProperty(() => _entity.What, value);
        }

        public double Amount
        {
            get => _entity.Amount;
            set => UpdateProperty(() => _entity.Amount, value);
        }

        public string Ccy
        {
            get => _entity.Ccy;
            set => UpdateProperty(() => _entity.Ccy, value);
        }

        public SmsModel Sms
        {
            get => Single<SmsModel>(_entity.SmsId);
            set => UpdateProperty(() => _entity.SmsId, value?.Id);
        }

        public SpentCategoryModel Category
        {
            get => Single<SpentCategoryModel>(_entity.CategoryId);
            set => UpdateProperty(() => _entity.CategoryId, value?.Id);
        }

        public string Provider
        {
            get => _entity.Provider;
            set => UpdateProperty(() => _entity.Provider, value);
        }

        public string Account
        {
            get => _entity.Account;
            set => UpdateProperty(() => _entity.Account, value);
        }

        public PaymentKind Kind
        {
            get => (PaymentKind) _entity.Kind;
            set => UpdateProperty(() => _entity.Kind, (int)value);
        }

        public string StatementReference
        {
            get => _entity.StatementReference;
            set => UpdateProperty(() => _entity.StatementReference, value);
        }
    }
}