using System;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;

namespace BudgetTracker.Model
{
    public sealed class SettingsModel : ModelBase
    {
        private readonly SettingsEntity _entity;

        public class SettingsEntity : BaseEntity
        {
            public string Password { get; set; }
        }

        public SettingsModel()
        {
            Id = Guid.NewGuid();
            _entity = new SettingsEntity
            {
                RowKey = Id.ToString(),
                PartitionKey = nameof(SettingsModel),
            };
        }

        public SettingsModel(SettingsEntity entity)
        {
            _entity = entity;
            Id = Guid.Parse(_entity.RowKey);
        }

        public override Guid Id { get; }
        protected override object Entity => _entity;

        public string Password
        {
            get => _entity.Password;
            set => UpdateProperty(() => _entity.Password, value);
        }
    }
}