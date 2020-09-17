using System.Linq;
using System.Threading.Tasks;
using BudgetTracker.Model;
using Microsoft.Extensions.Logging;

namespace BudgetTracker.GrpcServices
{
    public class MoneyColumnMetadatasViewModel : GrpcViewModelBase<MoneyColumnMetadataStream>
    {
        private ServerObservableCollection<MoneyColumnMetadataModel, MoneyColumnMetadataStream> _collection;

        public MoneyColumnMetadatasViewModel(ObjectRepository objectRepository, ILogger<SpentCategoriesViewModel> logger) : base(objectRepository, logger)
        {
        }

        protected override Task Init()
        {
            _collection = new ObjectRepositoryServerObservableCollection<MoneyColumnMetadataModel, MoneyColumnMetadataStream>(
                ObjectRepository,
                SendUpdate,
                (x, i) => new MoneyColumnMetadataStream {Added = ToStream(x)},
                (x, i) => new MoneyColumnMetadataStream {Removed = ToStream(x)},
                (x, i) => new MoneyColumnMetadataStream {Updated = ToStream(x)},
                list =>
                {
                    var model = new MoneyColumnMetadataStream {Snapshot = new MoneyColumnMetadataList()};
                    model.Snapshot.MoneyColumnMetadatas.AddRange(list.Select(ToStream).ToList());
                    return model;
                });
            
            Anchors.Add(() => _collection.Dispose());
            
            return Task.CompletedTask;
        }

        private MoneyColumnMetadata ToStream(MoneyColumnMetadataModel p0)
        {
            return new MoneyColumnMetadata
            {
                Id = p0.Id.ToUUID(),
                Function = p0.Function ?? "",
                Order = p0.Order,
                Provider = p0.Provider ?? "",
                AccountName = p0.AccountName ?? "",
                AutogenerateStatements = p0.AutogenerateStatements,
                UserFriendlyName = p0.UserFriendlyName ?? ""
            };
        }
    }
}