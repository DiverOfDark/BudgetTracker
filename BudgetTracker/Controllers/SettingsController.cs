using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;
using BudgetTracker.Scrapers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.Controllers
{
    [Authorize, AjaxOnlyActions]
    public class SettingsController : Controller
    {
        private readonly ObjectRepository _objectRepository;

        public SettingsController(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        public IndexViewModel IndexJson()
        {
            var scrapers = HttpContext.RequestServices.GetService<IEnumerable<GenericScraper>>();

            return new IndexViewModel(scrapers, _objectRepository);
        }

        [HttpPost]
        public OkResult UpdatePassword(string newPassword)
        {
            Startup.GlobalSettings.Password = newPassword;
            return Ok();
        }

        [HttpPost]
        public OkResult AddScraper(string name, string login, string password)
        {
            var scm = new ScraperConfigurationModel(name)
            {
                Login = login,
                Password = password
            };

            _objectRepository.Add(scm);

            return Ok();
        }

        [HttpPost]
        public OkResult DeleteConfig(Guid id)
        {
            _objectRepository.Remove<ScraperConfigurationModel>(v=>v.Id == id);

            return Ok();
        }

        [HttpPost]
        public OkResult ClearLastSuccessful(Guid id)
        {
            var model = _objectRepository.Set<ScraperConfigurationModel>().First(v => v.Id == id);
            model.LastSuccessfulBalanceScraping = default;
            model.LastSuccessfulStatementScraping = default;

            return Ok();
        }

        [ExportJsModel]
        public class IndexViewModel
        {
            public IndexViewModel(IEnumerable<GenericScraper> scrapers, ObjectRepository repository)
            {
                PossibleScrapers = scrapers.Select(v => v.ProviderName).OrderBy(v => v).ToList();
                ScraperConfigs = repository.Set<ScraperConfigurationModel>().Select(v=>new ScraperConfigurationJsModel(v)).ToList();
            }

            public bool CanDownloadDbDump => !string.IsNullOrWhiteSpace(Startup.DbFileName);
            
            public IEnumerable<string> PossibleScrapers { get; }
            
            public IEnumerable<ScraperConfigurationJsModel> ScraperConfigs { get; }
        }

        [ExportJsModel]
        public class ScraperConfigurationJsModel
        {
            private readonly ScraperConfigurationModel _scraperConfigurationModel;

            public ScraperConfigurationJsModel(ScraperConfigurationModel scraperConfigurationModel)
            {
                _scraperConfigurationModel = scraperConfigurationModel;
            }

            public Guid Id => _scraperConfigurationModel.Id;
            public string ScraperName => _scraperConfigurationModel.ScraperName;
            public string Login => _scraperConfigurationModel.Login;
            public string Password => string.IsNullOrWhiteSpace(_scraperConfigurationModel.Password) ? "" : "********" + new String(_scraperConfigurationModel.Password.TakeLast(2).ToArray());
            public string LastSuccessfulBalanceScraping => _scraperConfigurationModel.LastSuccessfulBalanceScraping != default ? _scraperConfigurationModel.LastSuccessfulBalanceScraping.ToString("g") : "-";
            public string LastSuccessfulStatementScraping => _scraperConfigurationModel.LastSuccessfulStatementScraping != default ? _scraperConfigurationModel.LastSuccessfulStatementScraping.ToString("g") : "-";
        }
    }
}