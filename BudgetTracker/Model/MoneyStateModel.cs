using System;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;

namespace BudgetTracker.Model
{
    public sealed class MoneyStateModel : ModelBase
    {
        public class MoneyStateEntity : BaseEntity
        {
            public string Provider { get; set; }
            public string AccountName { get; set; }
            public double Amount { get; set; }
            public string Ccy { get; set; }

            public DateTime When { get; set; }
        }
        
        private readonly MoneyStateEntity _entity;

        public MoneyStateModel(MoneyStateEntity entity)
        {
            _entity = entity;
            Id = Guid.Parse(_entity.RowKey);
        }

        public MoneyStateModel()
        {
            Id = Guid.NewGuid();
            _entity = new MoneyStateEntity
            {
                When = DateTime.UtcNow.Date,
                PartitionKey = nameof(MoneyStateModel),
                RowKey = Id.ToString()
            };
        }

        public override Guid Id { get; }
        protected override object Entity => _entity;

        public string Provider
        {
            get => _entity.Provider;
            set => UpdateProperty(() => _entity.Provider, value);
        }

        public string AccountName
        {
            get => _entity.AccountName;
            set => UpdateProperty(() => _entity.AccountName, value);
        }

        public double Amount
        {
            get => _entity.Amount;
            set => UpdateProperty(() => _entity.Amount, value);
        }

        public string Ccy
        {
            get => _entity.Ccy;
            set => UpdateProperty(() => _entity.Ccy, NormalizeCcy(value));
        }

        private string NormalizeCcy(string value)
        {
            if (value.ToLower().Contains("р"))
                return "RUB";
            if (value.ToLower().Contains("$"))
                return "USD";
            if (value.ToLower().Contains("€"))
                return "EUR";

            return value;
        }

        public DateTime When
        {
            get => _entity.When;
            set => UpdateProperty(() => _entity.When, value);
        }

        public override string ToString() => $"@{When.ToShortDateString()}: {Provider}/{AccountName}: {Amount} {Ccy}";
    }
}