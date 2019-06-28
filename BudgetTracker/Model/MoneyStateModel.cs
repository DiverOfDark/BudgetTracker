using System;
using System.Linq;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;

namespace BudgetTracker.Model
{
    public sealed class MoneyStateModel : ModelBase
    {
        public class MoneyStateEntity : BaseEntity
        {
            public Guid? ColumnId { get; set; }
            
            public string Provider { get; set; }
            public string AccountName { get; set; }
            public double Amount { get; set; }
            public string Ccy { get; set; }

            public DateTime When { get; set; }
        }
        
        private readonly MoneyStateEntity _entity;
        private MoneyColumnMetadataModel _lastColumn;

        public MoneyStateModel(MoneyStateEntity entity)
        {
            _entity = entity;
        }

        public MoneyStateModel()
        {
            _entity = new MoneyStateEntity
            {
                Id = Guid.NewGuid(),
                When = DateTime.UtcNow.Date,
            };
        }

        protected override BaseEntity Entity => _entity;

        internal void MigrateColumn()
        {
            if (_entity.ColumnId == null && !string.IsNullOrWhiteSpace(_entity.Provider) &&
                !string.IsNullOrWhiteSpace(_entity.AccountName))
            {
                Column = ObjectRepository.Set<MoneyColumnMetadataModel>().First(v =>
                    v.Provider == _entity.Provider && v.AccountName == _entity.AccountName);
                UpdateProperty(() => _entity.Provider, null);
                UpdateProperty(() => _entity.AccountName, null);
            }
        }
        
        public MoneyColumnMetadataModel Column
        {
            get => ObjectRepository == null ? _lastColumn : Single<MoneyColumnMetadataModel>(_entity.ColumnId);
            set
            {
                _lastColumn = value;
                UpdateProperty(() => _entity.ColumnId, value.Id); 
            }
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

        public DateTime When
        {
            get => _entity.When;
            set => UpdateProperty(() => _entity.When, value);
        }

        public override string ToString() => $"@{When.ToShortDateString()}: {Column.Provider}/{Column.UserFriendlyName}: {Amount} {Ccy}";
    }
}