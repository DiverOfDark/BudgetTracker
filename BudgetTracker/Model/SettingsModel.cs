using System;
using OutCode.EscapeTeams.ObjectRepository;

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
            _entity = new SettingsEntity
            {
                Id = Guid.NewGuid()
            };
        }

        public SettingsModel(SettingsEntity entity)
        {
            _entity = entity;
        }

        protected override BaseEntity Entity => _entity;

        public string Password
        {
            get => _entity.Password;
            set => UpdateProperty(_entity, () => x => x.Password, value);
        }
    }
}