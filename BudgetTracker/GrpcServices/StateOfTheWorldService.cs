using System;
using System.Threading.Tasks;
using Grpc.Core;
using JetBrains.Annotations;

namespace BudgetTracker.GrpcServices
{
    [UsedImplicitly]
    public class StateOfTheWorldService : SoWService.SoWServiceBase
    {
        private static Task<Empty> Empty = Task.FromResult(new Empty());
        
        private readonly SystemInfoProvider _systemInfoProvider;
        private readonly SettingsProvider _settingsProvider;

        public StateOfTheWorldService(SystemInfoProvider systemInfoProvider, SettingsProvider settingsProvider)
        {
            _systemInfoProvider = systemInfoProvider;
            _settingsProvider = settingsProvider;
        }

        public override async Task GetSystemInfo(Empty request, IServerStreamWriter<SystemInfo> responseStream, ServerCallContext context) => await _systemInfoProvider.Send(responseStream, context);

        public override async Task GetSettings(Empty request, IServerStreamWriter<Settings> responseStream, ServerCallContext context) => await _settingsProvider.Send(responseStream, context);

        public override Task<Empty> AddScraper(AddScraperRequest request, ServerCallContext context)
        {
            _settingsProvider.AddScraper(request.Name, request.Login, request.Password);
            return Empty;
        }

        public override Task<Empty> DeleteConfig(UUID request, ServerCallContext context)
        {
            _settingsProvider.DeleteConfig(request.ToGuid());
            return Empty;
        }

        public override Task<Empty> ClearLastSuccesful(UUID request, ServerCallContext context)
        {
            _settingsProvider.ClearLastSuccessful(request.ToGuid());
            return Empty;
        }

        public override Task<Empty> UpdateSettingsPassword(UpdatePasswordRequest request, ServerCallContext context)
        {
            _settingsProvider.UpdatePassword(request.NewPassword);
            return Empty;
        }
    }
}