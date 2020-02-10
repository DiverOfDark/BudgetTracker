using System;
using System.Threading.Tasks;
using BudgetTracker.Services;
using Google.Protobuf;
using Grpc.Core;
using Nito.AsyncEx;

namespace BudgetTracker.GrpcServices
{
    public abstract class GrpcModelProvider<T>: ViewModelBase where T : IMessage<T>
    {
        protected readonly AsyncManualResetEvent SendModelEvent = new AsyncManualResetEvent(true);

        protected T Model { get; set; }

        protected virtual Task Init() => Task.CompletedTask;

        public async Task Send(IServerStreamWriter<T> writer, ServerCallContext context) 
        {
            await Init();
            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (SendModelEvent.IsSet)
                {
                    SendModelEvent.Reset();
                    await writer.WriteAsync(Model);
                }

                await SendModelEvent.WaitAsync(context.CancellationToken);
            }
        }
    }
}