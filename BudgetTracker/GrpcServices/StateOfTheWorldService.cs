using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetTracker.Controllers;
using Google.Protobuf;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using FileMode = System.IO.FileMode;

namespace BudgetTracker.GrpcServices
{
    // It is all gathered in single service because we want to have single websocket connection 
    [UsedImplicitly]
    public class StateOfTheWorldService : SoWService.SoWServiceBase
    {
        private readonly GrpcProvider _provider;
        private readonly IHttpContextAccessor _accessor;
        private static readonly Task<Empty> Empty = Task.FromResult(new Empty());

        public StateOfTheWorldService(GrpcProvider provider, IHttpContextAccessor accessor)
        {
            _provider = provider;
            _accessor = accessor;
        }

        public override Task GetSystemInfo(Empty request, IServerStreamWriter<SystemInfo> responseStream, ServerCallContext context) => _provider.GetService<SystemInfoViewModel>(_accessor).Send(responseStream, context);

        public override Task GetSettings(Empty request, IServerStreamWriter<Settings> responseStream, ServerCallContext context) => _provider.GetService<SettingsViewModel>(_accessor).Send(responseStream, context);

        public override Task GetScreenshot(Empty request, IServerStreamWriter<Screenshot> responseStream, ServerCallContext context) => _provider.GetService<ScreenshotViewModel>(_accessor).Send(responseStream, context);

        public override Task GetDebts(Empty request, IServerStreamWriter<DebtsStream> responseStream, ServerCallContext context) => _provider.GetService<DebtsViewModel>(_accessor).Send(responseStream, context);

        public override Task GetPayments(Empty request, IServerStreamWriter<PaymentsStream> responseStream, ServerCallContext context) => _provider.GetService<PaymentsViewModel>(_accessor).Send(responseStream, context);

        public override Task GetSpentCategories(Empty request, IServerStreamWriter<SpentCategoriesStream> responseStream, ServerCallContext context) => _provider.GetService<SpentCategoriesViewModel>(_accessor).Send(responseStream, context);

        public override Task<ExecuteScriptResponse> ExecuteScript(ExecuteScriptRequest request, ServerCallContext context) => _provider.GetService<ScriptService>(_accessor).Evaluate(request);

        public override async Task<DbDump> DownloadDbDump(Empty request, ServerCallContext serverCallContext)
        {
            // TODO reimplement using streaming
            await using var fs = new FileStream(Startup.DbFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var contents = await ByteString.FromStreamAsync(fs);
            return new DbDump {Content = contents};

        }

        public override Task<Empty> AddScraper(AddScraperRequest request, ServerCallContext context)
        {
            _provider.GetService<SettingsViewModel>(_accessor).AddScraper(request.Name, request.Login, request.Password);
            return Empty;
        }

        public override Task<Empty> DeleteConfig(UUID request, ServerCallContext context)
        {
            _provider.GetService<SettingsViewModel>(_accessor).DeleteConfig(request.ToGuid());
            return Empty;
        }

        public override Task<Empty> ClearLastSuccesful(UUID request, ServerCallContext context)
        {
            _provider.GetService<SettingsViewModel>(_accessor).ClearLastSuccessful(request.ToGuid());
            return Empty;
        }

        public override Task<Empty> UpdateSettingsPassword(UpdatePasswordRequest request, ServerCallContext context)
        {
            _provider.GetService<SettingsViewModel>(_accessor).UpdatePassword(request.NewPassword);
            return Empty;
        }

        public override Task<Empty> EditDebt(Debt request, ServerCallContext context)
        {
            _provider.GetService<DebtsViewModel>(_accessor).EditDebt(request);
            return Empty;
        }

        public override Task<Empty> DeleteDebt(UUID request, ServerCallContext context)
        {
            _provider.GetService<DebtsViewModel>(_accessor).DeleteDebt(request);
            return Empty;
        }

        public override Task<PaymentDetails> GetPaymentDetails(UUID request, ServerCallContext context)
        {
            var result = _provider.GetService<PaymentsViewModel>(_accessor).GetPaymentDetails(request);
            return Task.FromResult(result);
        }

        public override Task<Empty> ShowCategorized(ShowCategorizedRequest request, ServerCallContext context)
        {
            _provider.GetService<PaymentsViewModel>(_accessor).UpdateShowCategorized(request.ShowCategorized);
            return Empty;
        }

        public override Task<Empty> ExpandCollapse(ExpandCollapse request, ServerCallContext context)
        {
            _provider.GetService<PaymentsViewModel>(_accessor).ExpandCollapseGroup(request.Path.Select(v=>v.ToGuid()).ToList());
            return Empty;
        }

        public override Task<Empty> DeletePayment(UUID request, ServerCallContext context)
        {
            _provider.GetService<PaymentsViewModel>(_accessor).DeletePayment(request);
            return Empty;
        }

        public override Task<Empty> EditPayment(Payment request, ServerCallContext context)
        {
            _provider.GetService<PaymentsViewModel>(_accessor).EditPayment(request);
            return Empty;
        }

        public override Task<Empty> SplitPayment(SplitPaymentRequest request, ServerCallContext context)
        {
            _provider.GetService<PaymentsViewModel>(_accessor).SplitPayment(request);
            return Empty;
        }

        public override Task<Empty> DeleteSpentCategory(UUID request, ServerCallContext context)
        {
            _provider.GetService<SpentCategoriesViewModel>(_accessor).DeleteCategory(request);
            return Empty;
        }

        public override Task<Empty> EditSpentCategory(SpentCategory request, ServerCallContext context)
        {
            _provider.GetService<SpentCategoriesViewModel>(_accessor).EditCategory(request);
            return Empty;
        }
    }
}