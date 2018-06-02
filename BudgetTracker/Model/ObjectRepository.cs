using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;

namespace BudgetTracker.Model
{
    public class ObjectRepository : ObjectRepositoryBase
    {
        public ObjectRepository(IStorage storage, ILoggerFactory logger) : base(storage, logger.CreateLogger(nameof(ObjectRepository)))
        {
            IsReadOnly = !Startup.IsProduction;
            AddType((MoneyStateModel.MoneyStateEntity x) => new MoneyStateModel(x));
            AddType((MoneyColumnMetadataModel.MoneyColumnMetadataEntity x) => new MoneyColumnMetadataModel(x));
            AddType((SmsModel.SmsEntity x) => new SmsModel(x));
            AddType((PaymentModel.PaymentEntity x) => new PaymentModel(x));
            AddType((RuleModel.RuleEntity x) => new RuleModel(x));
            AddType((SpentCategoryModel.SpendCategoryEntity x) => new SpentCategoryModel(x));
            AddType((WidgetModel.WidgetEntity x) => new WidgetModel(x));
            AddType((ScraperConfigurationModel.ScraperConfigurationEntity x) => new ScraperConfigurationModel(x));
            AddType((SettingsModel.SettingsEntity x) => new SettingsModel(x));
            Initialize();

#pragma warning disable 612
            Migration1();
            Migration2();
#pragma warning restore 612
        }

        [Obsolete]
        private void Migration1()
        {
            foreach (PaymentModel item in Set<PaymentModel>().Where(v => v.Column == null))
            {
                if (!string.IsNullOrWhiteSpace(item.OldProvider) && !string.IsNullOrWhiteSpace(item.OldAccount))
                {
                    var column = Set<MoneyColumnMetadataModel>().FirstOrDefault(v =>
                        v.Provider == item.OldProvider && v.AccountName == item.OldAccount);

                    if (column == null)
                    {
                        column = new MoneyColumnMetadataModel(item.OldProvider, item.OldAccount);
                        Add(column);
                    }

                    item.Column = column;
                    item.OldProvider = null;
                    item.OldAccount = null;
                }
            }
        }

        [Obsolete]
        private void Migration2()
        {
            foreach (PaymentModel model in Set<PaymentModel>())
            {
                if (model.Amount < 0)
                {
                    model.Amount = Math.Abs(model.Amount);
                }
            }
        }

        public string ExportDiff() => ((AzureTableContext)Storage).ExportStream();
    }
}