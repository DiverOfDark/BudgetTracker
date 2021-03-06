﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Model
{
    public sealed class ScraperConfigurationModel : ModelBase
    {
        private readonly ScraperConfigurationEntity _entity;

        public class ScraperConfigurationEntity : BaseEntity
        {
            public string ScraperName { get; set; }

            public string Login { get; set; }
            public string Password { get; set; }

            public string Properties { get; set; }

            public DateTime LastSuccessfulBalanceScraping { get; set; }
            public DateTime LastSuccessfulStatementScraping { get; set; }
        }

        public ScraperConfigurationModel(ScraperConfigurationEntity entity)
        {
            _entity = entity;
        }

        public ScraperConfigurationModel(String scraper)
        {
            _entity = new ScraperConfigurationEntity
            {
                Id = Guid.NewGuid(),
                ScraperName = scraper,
            };
        }

        protected override BaseEntity Entity => _entity;

        public string ScraperName => _entity.ScraperName;

        public ReadOnlyDictionary<string, string> Properties
        {
            get
            {
                try
                {
                    return new ReadOnlyDictionary<string, string>(
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(_entity.Properties));
                }
                catch
                {
                    return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
                }
            }
        }

        public void SetProperties(Dictionary<string, string> properties) =>
            UpdateProperty(_entity, () => x => x.Properties, JsonConvert.SerializeObject(properties));

        public string Login
        {
            get => _entity.Login;
            set => UpdateProperty(_entity, () => x => x.Login, value);
        }

        public string Password
        {
            get => _entity.Password;
            set => UpdateProperty(_entity, () => x => x.Password, value);
        }

        public DateTime LastSuccessfulBalanceScraping
        {
            get => _entity.LastSuccessfulBalanceScraping;
            set => UpdateProperty(_entity, () => x => x.LastSuccessfulBalanceScraping, value);
        }

        public DateTime LastSuccessfulStatementScraping
        {
            get => _entity.LastSuccessfulStatementScraping;
            set => UpdateProperty(_entity, () => x => x.LastSuccessfulStatementScraping, value);
        }
    }
}