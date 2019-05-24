using System.Threading.Tasks;
using BudgetTracker.Model;
using BudgetTracker.Services;

namespace BudgetTracker.JsModel
{
    [ExportJsModel]
    public class SystemInfo
    {
        public static async Task<SystemInfo> Create(UpdateService updateService, ObjectRepository objectRepository)
        {
            return new SystemInfo
            {
                IsProduction = Startup.IsProduction,
                CurrentVersion = Startup.CommmitHash,
                HasNewerVersion = await updateService.HasNewerVersion(),
                LatestVersion = await updateService.GetLatestVersion(),
                LaunchTime = Startup.LaunchTime.ToLocalTime().ToString("G"),
                Stats = objectRepository.Stats()
            };
        }

        private SystemInfo()
        {
        }

        public bool IsProduction { get; set; }
        public string LaunchTime { get; set; }
        public string Stats { get; set; }

        public string CurrentVersion { get; set; }
        public string LatestVersion { get; set; }
        public bool HasNewerVersion { get; set; }
    }
}