using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Services;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Nito.AsyncEx;

namespace BudgetTracker.GrpcServices
{
    public abstract class GrpcViewModelBase : ViewModelBase
    {
        private static readonly MemoryCache UserSessions = new MemoryCache(new MemoryCacheOptions());

        public GrpcViewModelBase(IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var cacheKey = httpContext.Session.GetString("cacheKey");

            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                cacheKey = Guid.NewGuid().ToString();
                httpContext.Session.SetString("cacheKey", cacheKey);
            }

            var vm = UserSessions.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(15);
                var created = new Dictionary<string, object>();
                return created;
            });

            UserSession = vm;
        }

        protected virtual Task Init() => Task.CompletedTask;

        protected Dictionary<string, object> UserSession { get; }
    }
    
    public abstract class GrpcViewModelBase<T>: GrpcViewModelBase where T : IMessage<T>
    {
        private readonly AsyncManualResetEvent _sendModelEvent = new AsyncManualResetEvent(true);

        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        protected GrpcViewModelBase(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
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
                while (!context.CancellationToken.IsCancellationRequested)
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