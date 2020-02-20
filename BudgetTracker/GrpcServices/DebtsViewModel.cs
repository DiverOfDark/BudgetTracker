using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BudgetTracker.Model;
using Microsoft.AspNetCore.Http;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.GrpcServices
{
    public class DebtsViewModel : GrpcViewModelBase<DebtsStream>
    {
        private readonly ObjectRepository _repository;

        public DebtsViewModel(ObjectRepository repository, IHttpContextAccessor accessor): base(accessor)
        {
            _repository = repository;
        }

        protected override Task Init()
        {
            SendSnapshot();
            _repository.ModelChanged += Handler;
            Anchors.Add(() => _repository.ModelChanged -= Handler);
            return Task.CompletedTask;
        }

        private void Handler(ModelChangedEventArgs obj)
        {
            // TODO debounce
            if (obj.Source is DebtModel || obj.Source is PaymentModel)
            {
                if (obj.Source is DebtModel debt)
                {
                    switch (obj.ChangeType)
                    {
                        case ChangeType.Update:
                            SendUpdate(new DebtsStream {Updated = ToDebtView(debt)});
                            break;
                        case ChangeType.Add:
                            SendUpdate(new DebtsStream {Added = ToDebtView(debt)});
                            break;
                        case ChangeType.Remove:
                            SendUpdate(new DebtsStream {Removed = ToDebtView(debt)});
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    SendSnapshot();
                }
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

        private void SendSnapshot()
        {
            var model = new DebtsStream {Snapshot = new DebtsList()};
            model.Snapshot.Debts.AddRange(_repository.Set<DebtModel>().Select(ToDebtView).ToList());
            SendUpdate(model);
        }

        public void EditDebt(Debt request)
        {
            DebtModel model;
            if (request.Id.ToGuid() == Guid.Empty)
            {
                model = new DebtModel();
                _repository.Add(model);
            }
            else
            {
                model = _repository.Set<DebtModel>().First(v => v.Id == request.Id.ToGuid());
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
            var debt = _repository.Set<DebtModel>().Find(request.ToGuid());
            if (debt != null)
            {
                foreach (var paymentModel in _repository.Set<PaymentModel>().Where(v=>v.Debt == debt))
                {
                    paymentModel.Debt = null;
                }

                _repository.Remove(debt);
            }
        }
    }
}