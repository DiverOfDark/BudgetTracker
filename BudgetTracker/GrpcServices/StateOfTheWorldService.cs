using System.Threading.Tasks;
using Grpc.Core;
using JetBrains.Annotations;

namespace BudgetTracker.GrpcServices
{
    [UsedImplicitly]
    public class StateOfTheWorldService : SoWService.SoWServiceBase
    {
        private static Task<Empty> Empty = Task.FromResult(new Empty());
        
        private readonly SystemInfoViewModel _systemInfoViewModel;
        private readonly SettingsViewModel _settingsViewModel;

        public StateOfTheWorldService(SystemInfoViewModel systemInfoViewModel, SettingsViewModel settingsViewModel)
        {
            _systemInfoViewModel = systemInfoViewModel;
            _settingsViewModel = settingsViewModel;
        }

        public override async Task GetSystemInfo(Empty request, IServerStreamWriter<SystemInfo> responseStream, ServerCallContext context) => await _systemInfoViewModel.Send(responseStream, context);

        public override async Task GetSettings(Empty request, IServerStreamWriter<Settings> responseStream, ServerCallContext context) => await _settingsViewModel.Send(responseStream, context);

        public override Task<Empty> AddScraper(AddScraperRequest request, ServerCallContext context)
        {
            _settingsViewModel.AddScraper(request.Name, request.Login, request.Password);
            return Empty;
        }

        public override Task<Empty> DeleteConfig(UUID request, ServerCallContext context)
        {
            _settingsViewModel.DeleteConfig(request.ToGuid());
            return Empty;
        }

        public override Task<Empty> ClearLastSuccesful(UUID request, ServerCallContext context)
        {
            _settingsViewModel.ClearLastSuccessful(request.ToGuid());
            return Empty;
        }

        public override Task<Empty> UpdateSettingsPassword(UpdatePasswordRequest request, ServerCallContext context)
        {
            _settingsViewModel.UpdatePassword(request.NewPassword);
            return Empty;
        }
    }
}