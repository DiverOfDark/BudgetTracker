using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Services;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace BudgetTracker.GrpcServices
{
    public abstract class GrpcViewModelBase<T>: ViewModelBase where T : IMessage<T>
    {
        private readonly AsyncManualResetEvent _sendModelEvent = new AsyncManualResetEvent(true);

        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        protected virtual Task Init() => Task.CompletedTask;

        public GrpcViewModelBase(ILogger logger)
        {
            Anchors.Add(() =>
            {
                logger.LogInformation($"Disposing {GetType().Name}.");
                _sendModelEvent.Set();
            });
        }
        
        protected void SendUpdate(T model)
        {
            _queue.Enqueue(model);
            _sendModelEvent.Set();
        }

        public async Task Send(IServerStreamWriter<T> writer, ServerCallContext context) 
        {
            await Init();
            try
            {
                while (!context.CancellationToken.IsCancellationRequested && !IsDisposed)
                {
                    if (_sendModelEvent.IsSet)
                    {
                        _sendModelEvent.Reset();
                        while (_queue.TryDequeue(out var item))
                        {
                            await writer.WriteAsync(item);
                        }
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