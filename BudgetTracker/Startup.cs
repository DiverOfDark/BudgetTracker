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
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;
using OutCode.EscapeTeams.ObjectRepository.Hangfire;
using OutCode.EscapeTeams.ObjectRepository.LiteDB;

namespace BudgetTracker
{
    public class Startup
    {
        private class ShouldSerializeContractResolver : CamelCasePropertyNamesContractResolver
        {
            public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                if (property.PropertyType != typeof(string) &&
                    typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    property.ShouldSerialize = instance =>
                    {
                        IEnumerable enumerable = null;
                        // this value could be in a public field or public property
                        switch (member.MemberType)
                        {
                            case MemberTypes.Property:
                                enumerable = instance
                                    .GetType()
                                    .GetProperty(member.Name)
                                    ?.GetValue(instance, null) as IEnumerable;
                                break;
                            case MemberTypes.Field:
                                enumerable = instance
                                    .GetType()
                                    .GetField(member.Name)
                                    .GetValue(instance) as IEnumerable;
                                break;
                        }

                        return enumerable == null ||
                               enumerable.GetEnumerator().MoveNext();
                        // if the list is null, we defer the decision to NullValueHandling
                    };
                }

                return property;
            }
        }
        
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

        public static string DbFileName { get; private set; }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.AddResponseCompression(x => x.EnableForHttps = true);
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.ContractResolver = ShouldSerializeContractResolver.Instance;
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
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
            services.AddHangfire(x=>{ });
            services.AddDataProtection().AddKeyManagementOptions(options =>
            {
                options.XmlRepository = new ObjectRepositoryXmlStorage(objectRepository);
            });
            services.AddNodeServices();
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
            
            app.UseSession();
            app.UseAuthentication();
            
            GlobalConfiguration.Configuration.UseHangfireStorage(app.ApplicationServices.GetService<ObjectRepository>());
            GlobalConfiguration.Configuration.UseActivator(new AspNetCoreJobActivator(new MyFactory(app.ApplicationServices)));

            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = new[]{new HttpContextAuth()},
                AppPath = null
            });
            app.UseHangfireServer();

            app.UseMvc(routes => routes.MapRoute(
                name: "not_so_default",
                template: "{controller}/{action}")
            );

            app.Use(async (a, b) =>
            {
                await a.Response.WriteAsync(File.ReadAllText("wwwroot/index.html"));
            });

            RegisterJobs(app.ApplicationServices);

            var storage = app.ApplicationServices.GetService<IStorage>();
            app.ApplicationServices.GetService<IApplicationLifetime>().ApplicationStopping.Register(() => storage.SaveChanges().GetAwaiter().GetResult());
            
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
