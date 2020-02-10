using System.Threading.Tasks;
using BudgetTracker.Model;
using BudgetTracker.Services;
using Grpc.Core;
using JetBrains.Annotations;

namespace BudgetTracker.GrpcServices
{
    [UsedImplicitly]
    public class StateOfTheWorldService : SoWService.SoWServiceBase
    {
        private readonly UpdateService _updateService;
        private readonly ObjectRepository _objectRepository;

        public StateOfTheWorldService(UpdateService updateService, ObjectRepository objectRepository)
        {
            _updateService = updateService;
            _objectRepository = objectRepository;
        }
        
        public override async Task<SystemInfo> GetSystemInfo(Empty request, ServerCallContext context)
        {
            return new SystemInfo
            {
                IsProduction = Startup.IsProduction,
                CurrentVersion = Startup.CommmitHash,
                HasNewerVersion = await _updateService.HasNewerVersion(),
                LatestVersion = await _updateService.GetLatestVersion(),
                LaunchTime = Startup.LaunchTime.ToLocalTime().ToString("G"),
                Stats = _objectRepository.Stats()
            };
        }
    }
}