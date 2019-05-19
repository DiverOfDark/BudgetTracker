using System.Threading.Tasks;
using BudgetTracker.Model;
using BudgetTracker.Services;
using ElectronNET.API;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    public class SystemController : Controller
    {
        private readonly UpdateService _updateService;
        private readonly ObjectRepository _objectRepository;

        public SystemController(UpdateService updateService, ObjectRepository objectRepository)
        {
            _updateService = updateService;
            _objectRepository = objectRepository;
        }
        
        public async Task<ActionResult> SiteInfo()
        {
            var result = new
            {
                IsProduction = Startup.IsProduction,
                CurrentVersion = await _updateService.GetCurrentVersion(),
                HasNewerVersion = await _updateService.HasNewerVersion(),
                LatestVersion = await _updateService.GetLatestVersion(),
                LatestVersionUrl = await _updateService.GetLatestVersionUrl(),
                LaunchTime = Startup.LaunchTime.ToLocalTime().ToString("G"),
                Stats = _objectRepository.Stats()
            };
            
            return Json(result);
        } 
    }
}