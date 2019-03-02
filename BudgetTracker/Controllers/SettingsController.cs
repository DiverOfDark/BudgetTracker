using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BudgetTracker.Model;
using BudgetTracker.Scrapers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ObjectRepository _objectRepository;

        public SettingsController(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        public IActionResult Index()
        {
            var scrapers = HttpContext.RequestServices.GetService<IEnumerable<GenericScraper>>();

            return View(new IndexViewModel(scrapers, _objectRepository));
        }

        public IActionResult UpdatePassword(string newPassword)
        {
            Startup.GlobalSettings.Password = newPassword;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AddScraper(string name, string login, string password)
        {
            var scm = new ScraperConfigurationModel(name)
            {
                Login = login,
                Password = password
            };

            _objectRepository.Add(scm);
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteConfig(Guid id)
        {
            _objectRepository.Remove<ScraperConfigurationModel>(v=>v.Id == id);
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ClearLastSuccessful(Guid id)
        {
            var model = _objectRepository.Set<ScraperConfigurationModel>().First(v => v.Id == id);
            model.LastSuccessfulBalanceScraping = default;
            model.LastSuccessfulStatementScraping = default;

            return RedirectToAction(nameof(Index));
        }

        public class IndexViewModel
        {
            public IndexViewModel(IEnumerable<GenericScraper> scrapers, ObjectRepository repository)
            {
                PossibleScrapers = scrapers.Select(v => v.ProviderName).OrderBy(v => v).ToList();
                ScraperConfigs = repository.Set<ScraperConfigurationModel>().ToList();
            }
            
            public IEnumerable<string> PossibleScrapers { get; }
            
            public IEnumerable<ScraperConfigurationModel> ScraperConfigs { get; }
        }

    }
}