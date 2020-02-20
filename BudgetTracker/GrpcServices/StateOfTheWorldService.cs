using System;
using System.IO;
using System.Threading.Tasks;
using BudgetTracker.Controllers;
using Google.Protobuf;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.GrpcServices
{
    [UsedImplicitly]
    public class StateOfTheWorldService : SoWService.SoWServiceBase
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly Task<Empty> Empty = Task.FromResult(new Empty());
        
        public StateOfTheWorldService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public override Task GetSystemInfo(Empty request, IServerStreamWriter<SystemInfo> responseStream, ServerCallContext context) => _serviceProvider.GetRequiredService<SystemInfoViewModel>().Send(responseStream, context);

        public override Task GetSettings(Empty request, IServerStreamWriter<Settings> responseStream, ServerCallContext context) => _serviceProvider.GetRequiredService<SettingsViewModel>().Send(responseStream, context);

        public override Task GetScreenshot(Empty request, IServerStreamWriter<Screenshot> responseStream, ServerCallContext context) => _serviceProvider.GetRequiredService<ScreenshotViewModel>().Send(responseStream, context);

        public override Task GetDebts(Empty request, IServerStreamWriter<DebtsStream> responseStream, ServerCallContext context) => _serviceProvider.GetRequiredService<DebtsViewModel>().Send(responseStream, context);
        
        public override Task<ExecuteScriptResponse> ExecuteScript(ExecuteScriptRequest request, ServerCallContext context) => _serviceProvider.GetRequiredService<ScriptService>().Evaluate(request);

        public override async Task<DbDump> DownloadDbDump(Empty request, ServerCallContext serverCallContext)
        {
            // TODO reimplement using streaming
            await using var fs = new FileStream(Startup.DbFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var contents = await ByteString.FromStreamAsync(fs);
            return new DbDump {Content = contents};

        }

        public override Task<Empty> AddScraper(AddScraperRequest request, ServerCallContext context)
        {
            _serviceProvider.GetRequiredService<SettingsViewModel>().AddScraper(request.Name, request.Login, request.Password);
            return Empty;
        }

        public override Task<Empty> DeleteConfig(UUID request, ServerCallContext context)
        {
            _serviceProvider.GetRequiredService<SettingsViewModel>().DeleteConfig(request.ToGuid());
            return Empty;
        }

        public override Task<Empty> ClearLastSuccesful(UUID request, ServerCallContext context)
        {
            _serviceProvider.GetRequiredService<SettingsViewModel>().ClearLastSuccessful(request.ToGuid());
            return Empty;
        }

        public override Task<Empty> UpdateSettingsPassword(UpdatePasswordRequest request, ServerCallContext context)
        {
            _serviceProvider.GetRequiredService<SettingsViewModel>().UpdatePassword(request.NewPassword);
            return Empty;
        }

        public override Task<Empty> EditDebt(Debt request, ServerCallContext context)
        {
            _serviceProvider.GetRequiredService<DebtsViewModel>().EditDebt(request);
            return Empty;
        }

        public override Task<Empty> DeleteDebt(UUID request, ServerCallContext context)
        {
            _serviceProvider.GetRequiredService<DebtsViewModel>().DeleteDebt(request);
            return Empty;
        }
    }
}