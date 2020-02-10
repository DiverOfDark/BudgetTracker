using System.Threading.Tasks;
using Grpc.Core;
using JetBrains.Annotations;

namespace BudgetTracker.GrpcServices
{
    [UsedImplicitly]
    public class StateOfTheWorldService : SoWService.SoWServiceBase
    {
        private readonly SystemInfoProvider _systemInfoProvider;

        public StateOfTheWorldService(SystemInfoProvider systemInfoProvider)
        {
            _systemInfoProvider = systemInfoProvider;
        }

        public override async Task GetSystemInfo(Empty request, IServerStreamWriter<SystemInfo> responseStream, ServerCallContext context)
        {
            await _systemInfoProvider.Send(responseStream, context);
        }
    }
}