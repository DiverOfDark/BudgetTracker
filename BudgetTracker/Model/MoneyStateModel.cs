using System;
using System.Linq;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Model
{
    public sealed class MoneyStateModel : ModelBase
    {
        public class MoneyStateEntity : BaseEntity
        {
            public Guid? ColumnId { get; set; }
            
            public double Amount { get; set; }
            public string Ccy { get; set; }

            public DateTime When { get; set; }
            public string Description { get; set; }
        }
        
        private readonly MoneyStateEntity _entity;

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

        private MoneyColumnMetadataModel _lastSetColumn = null;
        public MoneyColumnMetadataModel Column
        {
            get
            {
                var model = _lastSetColumn;
                if (model != null)
                {
                    return model;
                }

                model = Single<MoneyColumnMetadataModel>(_entity.ColumnId);
                return model;
            }
            set
            {
                UpdateProperty(_entity, () => x => x.ColumnId, (Guid?) value.Id);
                _lastSetColumn = value;
            }
        }

        public double Amount
        {
            get => _entity.Amount;
            set => UpdateProperty(_entity, () => x => x.Amount, value);
        }

        public string Ccy
        {
            get => _entity.Ccy;
            set => UpdateProperty(_entity, () => x => x.Ccy, CurrencyExtensions.NormalizeCcy(value));
        }

        public DateTime When
        {
            get => _entity.When;
            set => UpdateProperty(_entity, () => x => x.When, value);
        }

        public string Description
        {
            get => _entity.Description;
            set => UpdateProperty(_entity, () => x => x.Description, value);
        }

        public override string ToString() => $"@{When.ToShortDateString()}: {Column.Provider}/{Column.UserFriendlyName}: {Amount} {Ccy}";
    }
}