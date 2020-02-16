using System;
using System.Threading.Tasks;
using BudgetTracker.Services;
using Google.Protobuf;
using Grpc.Core;
using Nito.AsyncEx;

namespace BudgetTracker.GrpcServices
{
    public abstract class GrpcViewModelBase<T>: ViewModelBase where T : IMessage<T>
    {
        private readonly AsyncManualResetEvent _sendModelEvent = new AsyncManualResetEvent(true);

        protected void SendUpdate() => _sendModelEvent.Set();
        
        protected T Model { get; set; }

        protected virtual Task Init() => Task.CompletedTask;

        public async Task Send(IServerStreamWriter<T> writer, ServerCallContext context) 
        {
            await Init();
            try
            {
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    if (_sendModelEvent.IsSet)
                    {
                        _sendModelEvent.Reset();
                        await writer.WriteAsync(Model);
                    }

                    await _sendModelEvent.WaitAsync(context.CancellationToken);
                }
            } catch (Exception ex) when (ex is RpcException exception && exception.StatusCode == StatusCode.Cancelled || ex is TaskCanceledException)
            {
                // cancelled, not an issue                
            }
        }
    }
}