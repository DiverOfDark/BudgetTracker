using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetTracker.Model;
using BudgetTracker.Scrapers;
using Microsoft.Extensions.Logging;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.GrpcServices
{
    public class SettingsViewModel : GrpcViewModelBase<Settings>
    {
        private readonly List<string> _scrapers;
        private readonly Settings _model;

        public SettingsViewModel(ObjectRepository objectRepository, IEnumerable<GenericScraper> scrapers, ILogger<SettingsViewModel> logger) : base(objectRepository, logger)
        {
            _scrapers = scrapers.Select(v=>v.ProviderName).OrderBy(v=>v).ToList();
            _model = new Settings { CanDownloadDbDump = !string.IsNullOrWhiteSpace(Startup.DbFileName) };
        }

        protected override Task Init()
        {
            UpdateScraperConfigs();
            SendUpdate(_model);
            return Task.CompletedTask;
        }

        private void UpdateScraperConfigs()
        {
            string FormatDateTime(DateTime? from) => from == null ? "" : from.Value != default ? from.Value.ToString("g") : "-";

            _model.ScraperConfigs.Clear();
            var configs = ObjectRepository.Set<ScraperConfigurationModel>();
            foreach (string scraperName in _scrapers)
            {
                var config = configs.FirstOrDefault(v => v.ScraperName == scraperName);
                
                _model.ScraperConfigs.Add(new ScraperConfig
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

        protected override void OnModelRepositoryChanged(ModelChangedEventArgs obj)
        {
            if (obj.Source is ScraperConfigurationModel)
            {
                UpdateScraperConfigs();
                SendUpdate(_model);
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
            ObjectRepository.Add(scm);
        }

        public void DeleteConfig(Guid id)
        {
            ObjectRepository.Remove<ScraperConfigurationModel>(v=>v.Id == id);
        }

        public void ClearLastSuccessful(Guid id)
        {
            var model = ObjectRepository.Set<ScraperConfigurationModel>().First(v => v.Id == id);
            model.LastSuccessfulBalanceScraping = default;
            model.LastSuccessfulStatementScraping = default;
        }
    }
}