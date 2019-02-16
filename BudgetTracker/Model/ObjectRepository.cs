using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;
using OutCode.EscapeTeams.ObjectRepository.Hangfire;
using OutCode.EscapeTeams.ObjectRepository.LiteDB;

namespace BudgetTracker.Model
{
    public class ObjectRepository : ObjectRepositoryBase
    {
        public ObjectRepository(IStorage storage, ILoggerFactory logger) : base(storage, logger.CreateLogger(nameof(ObjectRepository)))
        {
            IsReadOnly = !Startup.IsProduction;
            AddType((MoneyStateModel.MoneyStateEntity x) => new MoneyStateModel(x));
            AddType((XmlKeyModel.XmlKeyEntity x) => new XmlKeyModel(x));
            AddType((MoneyColumnMetadataModel.MoneyColumnMetadataEntity x) => new MoneyColumnMetadataModel(x));
            AddType((SmsModel.SmsEntity x) => new SmsModel(x));
            AddType((PaymentModel.PaymentEntity x) => new PaymentModel(x));
            AddType((RuleModel.RuleEntity x) => new RuleModel(x));
            AddType((SpentCategoryModel.SpendCategoryEntity x) => new SpentCategoryModel(x));
            AddType((WidgetModel.WidgetEntity x) => new WidgetModel(x));
            AddType((ScraperConfigurationModel.ScraperConfigurationEntity x) => new ScraperConfigurationModel(x));
            AddType((SettingsModel.SettingsEntity x) => new SettingsModel(x));
            AddType((DebtModel.DebtEntity x) => new DebtModel(x));
            
            this.RegisterHangfireScheme();
            Initialize();
        }

        public string ExportDiff()
        {
            if (Storage is AzureTableContext)
            {
                return ((AzureTableContext) Storage).ExportStream();
            }

            if (Storage is LiteDbStorage)
            {
                return ((LiteDbStorage) Storage).ExportStream();
            }

            return null;
        }

        public string Stats() => String.Join("\n", _sets.Select(v=>v.Key.Name + $" ({v.Value.Count()})"));
    }
}