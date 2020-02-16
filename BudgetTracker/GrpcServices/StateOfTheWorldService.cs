using System;
using System.IO;
using System.Threading.Tasks;
using BudgetTracker.Controllers;
using Google.Protobuf;
using Grpc.Core;
using JetBrains.Annotations;

namespace BudgetTracker.GrpcServices
{
    [UsedImplicitly]
    public class StateOfTheWorldService : SoWService.SoWServiceBase
    {
        private static readonly Task<Empty> Empty = Task.FromResult(new Empty());
        
        private readonly SystemInfoViewModel _systemInfoViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly ScreenshotViewModel _screenshotViewModel;
        private readonly ScriptService _scriptService;

        public StateOfTheWorldService(SystemInfoViewModel systemInfoViewModel, SettingsViewModel settingsViewModel, ScreenshotViewModel screenshotViewModel, ScriptService scriptService)
        {
            _systemInfoViewModel = systemInfoViewModel;
            _settingsViewModel = settingsViewModel;
            _screenshotViewModel = screenshotViewModel;
            _scriptService = scriptService;
        }

        public override async Task GetSystemInfo(Empty request, IServerStreamWriter<SystemInfo> responseStream, ServerCallContext context) => await _systemInfoViewModel.Send(responseStream, context);

        public override async Task GetSettings(Empty request, IServerStreamWriter<Settings> responseStream, ServerCallContext context) => await _settingsViewModel.Send(responseStream, context);

        public override Task GetScreenshot(Empty request, IServerStreamWriter<Screenshot> responseStream, ServerCallContext context) => _screenshotViewModel.Send(responseStream, context);

        public override async Task<ExecuteScriptResponse> ExecuteScript(ExecuteScriptRequest request, ServerCallContext context) => await _scriptService.Evaluate(request);

        public override async Task<DbDump> DownloadDbDump(Empty request, ServerCallContext serverCallContext)
        {
            // TODO reimplement using streaming
            await using var fs = new FileStream(Startup.DbFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var contents = await ByteString.FromStreamAsync(fs);
            return new DbDump {Content = contents};

        }

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