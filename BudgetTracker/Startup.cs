using System;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading;
using BudgetTracker.Controllers;
using BudgetTracker.Model;
using BudgetTracker.Scrapers;
using BudgetTracker.Services;
using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;

namespace BudgetTracker
{
    public class Startup
    {
        private static readonly CultureInfo RussianCulture = new CultureInfo("ru-RU");

        public Startup(IConfiguration configuration)
        {
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                    RussianCulture;

            CultureInfo.CurrentCulture =
                CultureInfo.CurrentUICulture =
                    RussianCulture;
            
            Configuration = configuration;
            IsProduction = Configuration["Properties:IsProduction"] == "true";
            CommmitHash = Configuration["Properties:CiCommitHash"];
            CommmitName = Configuration["Properties:CiCommitName"];

            var instrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];

            if (instrumentationKey != null)
            {
                TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;
                TelemetryConfiguration.Active.DisableTelemetry = false;
            }
        }

        public static string CommmitName { get; private set; }
        public static string CommmitHash { get; private set; }
        public static bool IsProduction { get; private set; }
        public static DateTime LaunchTime { get; } = DateTime.UtcNow;
        public static SettingsModel GlobalSettings { get; private set; }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<Chrome>();
            services.AddSingleton<ScrapeService>();

            var connectionString = Configuration.GetConnectionString("AzureStorage");
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            var keysStorage = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference("xmlkeys");
            keysStorage.CreateIfNotExistsAsync().GetAwaiter().GetResult();

            var scrapers = GetType().Assembly.GetTypes().Where(v => v.IsSubclassOf(typeof(GenericScraper))).ToList();
            foreach (var s in scrapers)
            {
                services.AddTransient(typeof(GenericScraper), s);
            }
            
            services.AddWebEncoders(o =>
            {
                var textEncoderSettings = new TextEncoderSettings();
                textEncoderSettings.AllowRange(UnicodeRanges.All);
                o.TextEncoderSettings = textEncoderSettings;
            });
            services.AddSingleton(_ => cloudStorageAccount);
            services.AddSingleton(_ => _.GetRequiredService<CloudStorageAccount>().CreateCloudBlobClient());
            services.AddSingleton(_ => _.GetRequiredService<CloudStorageAccount>().CreateCloudTableClient());
            services.AddSingleton<IStorage, AzureTableContext>();
            services.AddSingleton<ObjectRepository>();
            services.AddSingleton<ScriptService>();
            services.AddSingleton<SmsRuleProcessor>();
            services.AddLogging();
            services.AddSession();
            services.AddHangfire(x=>{ });
            services.AddDataProtection().PersistKeysToAzureBlobStorage(keysStorage, "keys.xml");
            services.AddAuthorization();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(x =>
            {
                x.AccessDeniedPath = "/Auth";
                x.LoginPath = "/Auth";
                x.LogoutPath = "/Auth/Logout";
            });
        }

        private class MyFactory : IServiceScopeFactory
        {
            private readonly IServiceProvider _services;

            public MyFactory(IServiceProvider services) => _services = services;

            public IServiceScope CreateScope() => _services.CreateScope();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();
            
            var objRepoLogger = loggerFactory.CreateLogger("ObjectRepository");
            void OnError(Exception ex) => objRepoLogger.LogError(ex, ex.Message);
            
            app.ApplicationServices.GetService<IStorage>().OnError += OnError;

            var objectRepository = app.ApplicationServices.GetService<ObjectRepository>();
            objectRepository.OnException += OnError;

            while (objectRepository.IsLoading)
            {
                Thread.Sleep(50);
            }

            var settingsModel = objectRepository.Set<SettingsModel>().FirstOrDefault();
            if (settingsModel == null)
            {
                settingsModel = new SettingsModel();
                objectRepository.Add(settingsModel);
            }

            GlobalSettings = settingsModel;
            
            app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
            app.UseExceptionHandler("/Error");
            app.UseStaticFiles();
            app.UseSession();
            app.UseAuthentication();
            
            GlobalConfiguration.Configuration.UseMemoryStorage();
            GlobalConfiguration.Configuration.UseActivator(new AspNetCoreJobActivator(new MyFactory(app.ApplicationServices)));
            
            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = new[]{new HttpContextAuth()}
            });
            app.UseHangfireServer();

            app.UseMvc(routes => routes.MapRoute(
                    name: "not_so_default",
                    template: "{controller=Widget}/{action=Index}")
                .MapRoute(name: "error",
                    template: "Error",
                    defaults: new
                    {
                        controller = "Widget",
                        action = "Error"
                    }));

            RegisterJobs();
        }

        private void RegisterJobs()
        {
            var interval = IsProduction ? Cron.Hourly() : Cron.Yearly();
            
            RecurringJob.AddOrUpdate<ScrapeService>(x=>x.Scrape(), interval);
            RecurringJob.AddOrUpdate<RepositoryCleanupService>(x=>x.Run(), interval);
            RecurringJob.AddOrUpdate<SmsRuleProcessor>(x=>x.Process(), Cron.MinuteInterval(5));
        }
    }

    public class HttpContextAuth : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => context.GetHttpContext().User.Identity.IsAuthenticated;
    }
}
