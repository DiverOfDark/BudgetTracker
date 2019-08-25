using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
using LiteDB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;
using OutCode.EscapeTeams.ObjectRepository.Hangfire;
using OutCode.EscapeTeams.ObjectRepository.LiteDB;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

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
        }

        public static string CommmitName { get; private set; }
        public static string CommmitHash { get; private set; }
        public static bool IsProduction { get; private set; }
        public static DateTime LaunchTime { get; } = DateTime.UtcNow;
        public static SettingsModel GlobalSettings { get; private set; }

        public static string DbFileName { get; private set; }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.AddResponseCompression(x => x.EnableForHttps = true);
            services.AddMvc().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Formatting = IsProduction ? Formatting.None : Formatting.Indented;
            });
            services.AddSingleton<Chrome>();
            services.AddSingleton<ScrapeService>();

            ObjectRepository objectRepository;
            {
                var liteDb = Configuration.GetConnectionString("LiteDb");
                var azureDb = Configuration.GetConnectionString("AzureStorage");

                IStorage storage = null;
                
                if (!String.IsNullOrEmpty(liteDb))
                {
                    var connectionString = new ConnectionString(liteDb);
                    DbFileName = connectionString.Filename;
                    var liteDbDatabase = new LiteDatabase(connectionString);
                    liteDbDatabase.Engine.Shrink();
                    storage = new LiteDbStorage(liteDbDatabase);
                } else if (!String.IsNullOrEmpty(azureDb)) {
                    var cloudStorageAccount = CloudStorageAccount.Parse(azureDb);
                    storage = new AzureTableContext(cloudStorageAccount.CreateCloudTableClient());
                } else {
                    throw new Exception("Connection string for either 'AzureStorage' or 'LiteDb' should been specified.");
                }
                
                objectRepository = new ObjectRepository(storage, NullLoggerFactory.Instance);

                services.AddSingleton(storage);
                services.AddSingleton(objectRepository);
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
            services.AddSingleton<UpdateService>();
            services.AddLogging();
            services.AddSession();
            services.AddControllers().AddNewtonsoftJson();
            services.AddHangfire(x=>{ });
            services.AddDataProtection().AddKeyManagementOptions(options =>
            {
                options.XmlRepository = new ObjectRepositoryXmlStorage(objectRepository);
            });
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
            app.UseResponseCompression();
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
            
            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl = 
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                    {
                        NoCache = true
                    };

                await next();
            });
            
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            
            GlobalConfiguration.Configuration.UseHangfireStorage(app.ApplicationServices.GetService<ObjectRepository>());
            GlobalConfiguration.Configuration.UseActivator(new AspNetCoreJobActivator(new MyFactory(app.ApplicationServices)));

            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = new[]{new HttpContextAuth()},
                AppPath = null
            });
            app.UseHangfireServer();
            
            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute("not_so_default", "{controller}/{action}");
                app.Use(async (a, b) =>
                {
                    if (a.Response.StatusCode != 200)
                    {
                        await a.Response.WriteAsync("Response " + a.Response.StatusCode);
                    }
                    else
                    {
                        await b();
                    }
                });
                routes.MapFallbackToFile("index.html");
            });

            RegisterJobs(app.ApplicationServices);

            var storage = app.ApplicationServices.GetService<IStorage>();
            app.ApplicationServices.GetService<IHostApplicationLifetime>().ApplicationStopping.Register(() => storage.SaveChanges().GetAwaiter().GetResult());
            
            new Thread(()=>
            {
                var objectRepository = app.ApplicationServices.GetService<ObjectRepository>();
                objectRepository.OnException += OnError;

                objectRepository.WaitForInitialize().GetAwaiter().GetResult();

                var settingsModel = objectRepository.Set<SettingsModel>().FirstOrDefault();
                if (settingsModel == null)
                {
                    settingsModel = new SettingsModel();
                    objectRepository.Add(settingsModel);
                }

                GlobalSettings = settingsModel;
            }).Start();
        }

        private void RegisterJobs(IServiceProvider services)
        {
            var interval = IsProduction ? Cron.HourInterval(12) : Cron.Yearly();

            services.GetService<ScrapeService>().RegisterJobs(interval);
            RecurringJob.AddOrUpdate<RepositoryCleanupService>(x=>x.Run(), interval);
            RecurringJob.AddOrUpdate<SmsRuleProcessor>(x=>x.Process(), Cron.MinuteInterval(5));
            RecurringJob.AddOrUpdate<SpentCategoryProcessor>(x=>x.Process(), Cron.MinuteInterval(30));
        }
    }
}
