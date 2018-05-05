using System;
using System.Collections.Generic;
using System.Linq;
using OutCode.EscapeTeams.ObjectRepository;
using OutCode.EscapeTeams.ObjectRepository.AzureTableStorage;

namespace BudgetTracker.Model
{
    public static class Extensions
    {
        public static IEnumerable<MoneyColumnMetadataModel> SortColumns(this IEnumerable<MoneyColumnMetadataModel> self)
        {
            return self.OrderByDescending(v=>v.IsComputed).ThenBy(v => v.Provider).ThenBy(v => v.Order).ThenBy(v => v.AccountName);
        }
    }

    public sealed class MoneyColumnMetadataModel : ModelBase
    {
        public const string ComputedProdiver = "Computed";

        public class MoneyColumnMetadataEntity : BaseEntity
        {
            public string Provider { get; set; }
            public string AccountName { get; set; }

            public string UserFriendlyName { get; set; }
            public bool IsVisible { get; set; }

            public int Order { get; set; }

            public string Function { get; set; }
        }

        private readonly MoneyColumnMetadataEntity _entity;

        public MoneyColumnMetadataModel(MoneyColumnMetadataEntity entity)
        {
            _entity = entity;
            Id = Guid.Parse(_entity.RowKey);
        }

        public MoneyColumnMetadataModel(string provider, string accountName)
        {
            Id = Guid.NewGuid();
            _entity = new MoneyColumnMetadataEntity
            {
                AccountName = accountName,
                Provider = provider,
                PartitionKey = nameof(MoneyColumnMetadataEntity),
                RowKey = Id.ToString()
            };
        }

        protected override object Entity => _entity;

        public override Guid Id { get; }

        public string AccountName => _entity.AccountName;
        public string Provider => _entity.Provider;

        public string UserFriendlyName
        {
            get => _entity.UserFriendlyName;
            set => UpdateProperty(() => _entity.UserFriendlyName, value);
        }

        public bool IsVisible
        {
            get => _entity.IsVisible;
            set => UpdateProperty(() => _entity.IsVisible, value);
        }

        public int Order
        {
            get => _entity.Order;
            set => UpdateProperty(() => _entity.Order, value);
        }

        public bool IsComputed => Provider == ComputedProdiver;

        public string Function
        {
            get => _entity.Function;
            set => UpdateProperty(() => _entity.Function, value);
        }

        public IEnumerable<string> ChartList
        {
            get
            {
                if (!IsComputed || !Function.Contains('+'))
                    return Enumerable.Empty<string>();

                var result = new List<string>();

                var fn = Function;

                while (fn.Contains('[') && fn.Contains(']'))
                {
                    var start = fn.IndexOf('[');
                    var end = fn.IndexOf(']');
                    var substring = fn.Substring(start + 1, end - start - 1);
                    fn = fn.Substring(end + 1);
                    result.Add(substring);
                }

                return result;
            }
        }
    }
}