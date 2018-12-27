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
            public bool AutogenerateStatements { get; set; }

            public int Order { get; set; }

            public string Function { get; set; }
        }

        private readonly MoneyColumnMetadataEntity _entity;

        public MoneyColumnMetadataModel(MoneyColumnMetadataEntity entity)
        {
            _entity = entity;
        }

        public MoneyColumnMetadataModel(string provider, string accountName)
        {
            _entity = new MoneyColumnMetadataEntity
            {
                AccountName = accountName,
                Provider = provider,
                Id = Guid.NewGuid()
            };
        }

        protected override BaseEntity Entity => _entity;

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

        public bool AutogenerateStatements
        {
            get => _entity.AutogenerateStatements;
            set => UpdateProperty(() => _entity.AutogenerateStatements, value);
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
                if (!IsComputed)
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