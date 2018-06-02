using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;

namespace BudgetTracker.Model
{
    public enum PaymentKind
    {
        [DisplayName("Трата")]
        Expense = 0,
        [DisplayName("Доход")]
        Income = 1,
        [DisplayName("Перевод")]
        Transfer = 2,
        [DisplayName("Неизвестно")]
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
            
            [Obsolete]
            public string Provider { get; set; }
            [Obsolete]
            public string Account { get; set; }
            
            public Guid? ColumnId { get; set; }
            public int Kind { get; set; }
            public string StatementReference { get; set; }
        }
        
        private readonly PaymentEntity _entity;

        public PaymentModel(PaymentEntity entity)
        {
            _entity = entity;
            Id = Guid.Parse(_entity.RowKey);
        }

        public PaymentModel(DateTime when, string what, double amount, PaymentKind kind, string ccy, string statementReference,
            MoneyColumnMetadataModel column)
        {
            Id = Guid.NewGuid();
            _entity = new PaymentEntity
            {
                PartitionKey = nameof(MoneyStateModel),
                RowKey = Id.ToString(),
                When = when,
                Amount = amount,
                What = what,
                Kind = (int) kind,
                StatementReference = statementReference,
                ColumnId = column?.Id
            };
            Ccy = ccy;
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
                Kind = (int) PaymentKind.Expense
            };

            Amount = double.Parse(amount, new NumberFormatInfo
            {
                NumberDecimalSeparator = "."
            });
            What = matches.Groups.First(v => v.Name == "what").Value;
            Ccy = matches.Groups.First(v => v.Name == "ccy").Value;
        }

        public sealed override Guid Id { get; }
        protected override object Entity => _entity;

        public Guid? ColumnId => _entity.ColumnId;
        public Guid? CategoryId => _entity.CategoryId;
        public Guid? SmsId => _entity.SmsId;
        public DateTime When => _entity.When;

        [Obsolete]
        public string OldProvider
        {
            get => _entity.Provider;
            set => UpdateProperty(() => _entity.Provider, value);
        }

        [Obsolete]
        public string OldAccount
        {
            get => _entity.Account;
            set => UpdateProperty(() => _entity.Account, value);
        }

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
            set => UpdateProperty(() => _entity.Ccy, CurrencyExtensions.NormalizeCcy(value));
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

        public MoneyColumnMetadataModel Column
        {
            get => Single<MoneyColumnMetadataModel>(_entity.ColumnId);
            set => UpdateProperty(() => _entity.ColumnId, value?.Id);
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

        public double SignedAmount
        {
            get
            {
                switch (Kind)
                {
                    case PaymentKind.Expense:
                        return -Amount;

                    case PaymentKind.Income:
                        return Amount;

                    case PaymentKind.Transfer:
                        return Amount;

                    default:
                        return Amount;
                }
            }
        }
    }
}