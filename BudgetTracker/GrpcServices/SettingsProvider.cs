using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;
using BudgetTracker.Scrapers;
using Microsoft.AspNetCore.Identity;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.GrpcServices
{
    public class SettingsProvider : GrpcModelProvider<Settings>
    {
        private readonly ObjectRepository _objectRepository;

        public SettingsProvider(ObjectRepository objectRepository, IEnumerable<GenericScraper> scrapers)
        {
            _objectRepository = objectRepository;
            Model = new Settings { CanDownloadDbDump = !string.IsNullOrWhiteSpace(Startup.DbFileName) };
            Model.Scrapers.AddRange(scrapers.Select(v => v.ProviderName).OrderBy(v => v));
            UpdateScraperConfigs();
            
            _objectRepository.ModelChanged += ModelHandler;
            Anchors.Add(() => _objectRepository.ModelChanged -= ModelHandler);
        }

        private void UpdateScraperConfigs()
        {
            string FormatDateTime(DateTime from) => @from != default ? @from.ToString("g") : "-";

            foreach (ScraperConfigurationModel config in _objectRepository.Set<ScraperConfigurationModel>())
            {
                Model.ScraperConfigs.Add(new ScraperConfig
                {
                    Id = config.Id.ToUUID(),
                    LastSuccessfulBalanceScraping = FormatDateTime(config.LastSuccessfulBalanceScraping),
                    LastSuccessfulStatementScraping = FormatDateTime(config.LastSuccessfulStatementScraping),
                    Login = config.Login,
                    Password = string.IsNullOrWhiteSpace(config.Password)
                        ? ""
                        : "********" + new String(config.Password.TakeLast(2).ToArray()),
                    ScraperName = config.ScraperName
                });
            }
        }

        private void ModelHandler(ModelChangedEventArgs obj)
        {
            if (obj.Source is ScraperConfigurationModel)
            {
                UpdateScraperConfigs();
                SendUpdate();
            }
        }

        public void UpdatePassword(string newPassword)
        {
            Startup.GlobalSettings.Password = newPassword;
        }

        public void AddScraper(string name, string login, string password)
        {
            var scm = new ScraperConfigurationModel(name)
            {
                Login = login,
                Password = password
            };

            _objectRepository.Add(scm);
        }

        public void DeleteConfig(Guid id)
        {
            _objectRepository.Remove<ScraperConfigurationModel>(v=>v.Id == id);
        }

        public void ClearLastSuccessful(Guid id)
        {
            var model = _objectRepository.Set<ScraperConfigurationModel>().First(v => v.Id == id);
            model.LastSuccessfulBalanceScraping = default;
            model.LastSuccessfulStatementScraping = default;
        }
    }
}