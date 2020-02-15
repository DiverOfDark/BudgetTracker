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
        private readonly List<string> _scrapers;

        public SettingsProvider(ObjectRepository objectRepository, IEnumerable<GenericScraper> scrapers)
        {
            _objectRepository = objectRepository;
            _scrapers = scrapers.Select(v=>v.ProviderName).OrderBy(v=>v).ToList();
            
            Model = new Settings { CanDownloadDbDump = !string.IsNullOrWhiteSpace(Startup.DbFileName) };
            UpdateScraperConfigs();
            
            _objectRepository.ModelChanged += ModelHandler;
            Anchors.Add(() => _objectRepository.ModelChanged -= ModelHandler);
        }

        private void UpdateScraperConfigs()
        {
            string FormatDateTime(DateTime? from) => from == null ? "" : from.Value != default ? from.Value.ToString("g") : "-";

            Model.ScraperConfigs.Clear();
            var configs = _objectRepository.Set<ScraperConfigurationModel>();
            foreach (string scraperName in _scrapers)
            {
                var config = configs.FirstOrDefault(v => v.ScraperName == scraperName);
                
                Model.ScraperConfigs.Add(new ScraperConfig
                {
                    Id = config?.Id.ToUUID(),
                    LastSuccessfulBalanceScraping = FormatDateTime(config?.LastSuccessfulBalanceScraping),
                    LastSuccessfulStatementScraping = FormatDateTime(config?.LastSuccessfulStatementScraping),
                    Login = config == null ? "" : string.IsNullOrWhiteSpace(config?.Login) ? "<не указан>" : config.Login,
                    Password = config == null ? "" : string.IsNullOrWhiteSpace(config?.Password)
                        ? "<не указан>"
                        : "********" + new String(config.Password.TakeLast(2).ToArray()),
                    ScraperName = scraperName,
                    Enabled = config != null
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