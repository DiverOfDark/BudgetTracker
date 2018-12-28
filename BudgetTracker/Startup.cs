using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading;
using BudgetTracker.Controllers;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Model;
using BudgetTracker.Scrapers;
using BudgetTracker.Services;
using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Dashboard.Resources;
using Hangfire.MemoryStorage;
using LiteDB;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AzureStorage;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;
using OutCode.EscapeTeams.ObjectRepository.LiteDB;

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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.AddMvc();
            services.AddSingleton<Chrome>();
            services.AddSingleton<ScrapeService>();

            {
                var liteDb = Configuration.GetConnectionString("LiteDb");
                var azureDb = Configuration.GetConnectionString("AzureStorage");

                IStorage storage = null;
                
                if (!String.IsNullOrEmpty(liteDb))
                {
                    var liteDbDatabase = new LiteDatabase(liteDb);
                    storage = new LiteDbStorage(liteDbDatabase);
                } else if (!String.IsNullOrEmpty(azureDb))
                {
                    var cloudStorageAccount = CloudStorageAccount.Parse(azureDb);
                    storage = new AzureTableContext(cloudStorageAccount.CreateCloudTableClient());
                }
                else
                {
                    throw new Exception(
                        "Connection string for either 'AzureStorage' or 'LiteDb' should been specified.");
                }
                
                var objectRepository = new ObjectRepository(storage, NullLoggerFactory.Instance);

                services.AddSingleton(storage);
                services.AddSingleton(objectRepository);
                services.Configure((Action<KeyManagementOptions>) (options =>
                {
                    options.XmlRepository = new ObjectRepositoryXmlStorage(objectRepository);
                }));
            }
            
            var scrapers = GetType().Assembly.GetTypes().Where(v => v.IsSubclassOf(typeof(GenericScraper))).ToList();
            foreach (var s in scrapers)
            {
                services.AddSingleton(typeof(GenericScraper), s);
            }
            
            services.AddWebEncoders(o =>
            {
                var textEncoderSettings = new TextEncoderSettings();
                textEncoderSettings.AllowRange(UnicodeRanges.All);
                o.TextEncoderSettings = textEncoderSettings;
            });
            services.AddTransient(x => new TableViewModelFactory(x.GetRequiredService<ObjectRepository>()));
            services.AddSingleton<ScriptService>();
            services.AddSingleton<SmsRuleProcessor>();
            services.AddLogging();
            services.AddSession();
            services.AddHangfire(x=>{ });

            services.AddDataProtection();
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

            app.Use(async (a, b) =>
            {
                if (GlobalSettings == null)
                {
                    await a.Response.WriteAsync("Site is loading...");
                    return;
                }

                await b();
            });
            app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
            app.UseExceptionHandler("/Error");
            app.UseStaticFiles();
            app.UseSession();
            app.UseAuthentication();
            
            GlobalConfiguration.Configuration.UseMemoryStorage();
            GlobalConfiguration.Configuration.UseActivator(new AspNetCoreJobActivator(new MyFactory(app.ApplicationServices)));

            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = new[]{new HttpContextAuth()},
                AppPath = null
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
            
            
            new Thread(()=>
            {
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
            }).Start();
        }

        private void RegisterJobs()
        {
            var interval = IsProduction ? Cron.Hourly() : Cron.Yearly();
            
            RecurringJob.AddOrUpdate<ScrapeService>(x=>x.Scrape(), interval);
            RecurringJob.AddOrUpdate<RepositoryCleanupService>(x=>x.Run(), interval);
            RecurringJob.AddOrUpdate<SmsRuleProcessor>(x=>x.Process(), Cron.MinuteInterval(5));
            RecurringJob.AddOrUpdate<SpentCategoryProcessor>(x=>x.Process(), Cron.MinuteInterval(30));
        }
    }
}
