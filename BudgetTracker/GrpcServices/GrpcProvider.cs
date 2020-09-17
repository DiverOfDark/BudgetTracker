using System;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.GrpcServices
{
    public class GrpcProvider
    {
        private static readonly MemoryCache UserSessions = new MemoryCache(new MemoryCacheOptions());

        private readonly IServiceProvider _serviceProvider;

        public GrpcProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T GetService<T>(IHttpContextAccessor accessor) where T:ViewModelBase
        {
            var httpContext = accessor.HttpContext;
            httpContext.Request.Cookies.TryGetValue(".AspNetCore.Session", out var cookie);
            
            var vmKey = typeof(T).FullName + ":" + (cookie ?? httpContext.Session.Id);

            var vm = UserSessions.GetOrCreate(vmKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(15);
                var created = _serviceProvider.GetRequiredService<T>();
                entry.RegisterPostEvictionCallback((a, b, c, d) => created.Dispose());
                entry.Value = created;
                return created;
            });

            if (vm.IsDisposed)
            {
                UserSessions.Remove(vmKey);
                return GetService<T>(accessor);
            }

            return vm;
        }
    }
}