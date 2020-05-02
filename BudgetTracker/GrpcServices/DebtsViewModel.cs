using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BudgetTracker.Model;
using Microsoft.Extensions.Logging;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.GrpcServices
{
    public class DebtsViewModel : GrpcViewModelBase<DebtsStream>
    {
        private ObjectRepositoryServerObservableCollection<DebtModel, DebtsStream> _collection;

        public DebtsViewModel(ObjectRepository objectRepository, ILogger<DebtsViewModel> logger) : base(objectRepository, logger)
        {
        }

        protected override Task Init()
        {
            _collection = new ObjectRepositoryServerObservableCollection<DebtModel, DebtsStream>(
                ObjectRepository,
                SendUpdate,
                (x, i) => new DebtsStream {Added = ToDebtView(x)},
                (x, i)=>new DebtsStream {Removed = ToDebtView(x)},
                (x, i) => new DebtsStream {Updated = ToDebtView(x)},
                list =>
                {
                    var model = new DebtsStream {Snapshot = new DebtsList()};
                    model.Snapshot.Debts.AddRange(list.Select(ToDebtView).ToList());
                    return model;
                });
            Anchors.Add(_collection.Dispose);
            return Task.CompletedTask;
        }

        protected override void OnModelRepositoryChanged(ModelChangedEventArgs obj)
        {
            // TODO handle in debtmodel
            if (obj.Source is PaymentModel)
            {
                _collection.Dispose();
                Init();
            }
        }

        private DebtView ToDebtView(DebtModel debt)
        {
            return new DebtView
            {
                Model = new Debt
                {
                    Amount = debt.Amount,
                    Ccy = debt.Ccy ?? "",
                    Description = debt.Description ?? "",
                    Id = debt.Id.ToUUID(),
                    Issued = debt.When.ToTimestamp(),
                    Percentage = debt.Percentage,
                    DaysCount = debt.DaysCount,
                    RegexForTransfer = debt.RegexForTransfer ?? ""
                },
                Returned = debt.Returned,
                LastPaymentDate = debt.LastPaymentDate ?? ""
            };
        }

        public void EditDebt(Debt request)
        {
            DebtModel model;
            if (request.Id.ToGuid() == Guid.Empty)
            {
                model = new DebtModel();
                ObjectRepository.Add(model);
            }
            else
            {
                model = ObjectRepository.Set<DebtModel>().First(v => v.Id == request.Id.ToGuid());
            }

            model.When = request.Issued.ToDateTime();
            model.Amount = request.Amount;
            model.Ccy = request.Ccy;
            model.Percentage = request.Percentage;
            model.DaysCount = request.DaysCount;
            model.Description = request.Description;
            try
            {
                if (!string.IsNullOrWhiteSpace(request.RegexForTransfer))
                {
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    new Regex(request.RegexForTransfer, RegexOptions.None, TimeSpan.FromSeconds(0.1)).Match("test");
                }
            }
            catch
            {
                request.RegexForTransfer = "";
            }

            model.RegexForTransfer = request.RegexForTransfer;
        }

        public void DeleteDebt(UUID request)
        {
            var debt = ObjectRepository.Set<DebtModel>().Find(request.ToGuid());
            if (debt != null)
            {
                foreach (var paymentModel in ObjectRepository.Set<PaymentModel>().Where(v=>v.Debt == debt))
                {
                    paymentModel.Debt = null;
                }

                ObjectRepository.Remove(debt);
            }
        }
    }
}