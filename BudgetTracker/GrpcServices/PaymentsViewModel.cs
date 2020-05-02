using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetTracker.Model;
using BudgetTracker.Services;
using Microsoft.Extensions.Logging;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.GrpcServices
{
    public class PaymentsViewModel : GrpcViewModelBase<PaymentsStream>
    {
        public static class PaymentTransformers
        {
            public static PaymentsStream OnAdd(PaymentsMonthViewModel arg1, int arg2) =>
                new PaymentsStream
                {
                    Added = new PaymentViewUpdate
                    {
                        Position = arg2,
                        View = ToPaymentView(arg1)
                    }
                };

            public static PaymentsStream OnRemove(PaymentsMonthViewModel arg1, int arg2) =>
                new PaymentsStream
                {
                    Removed = new PaymentViewUpdate
                    {
                        Position = arg2
                    }
                };

            public static PaymentsStream OnUpdate(PaymentsMonthViewModel arg1, int arg2) =>
                new PaymentsStream
                {
                    Updated = new PaymentViewUpdate
                    {
                        Position = arg2,
                        View = ToPaymentView(arg1)
                    }
                };

            private static PaymentView ToPaymentView(PaymentsMonthViewModel arg1) => new PaymentView { Summary = ToMonthSummary(arg1) };

            public static MonthSummary ToMonthSummary(PaymentsMonthViewModel vm)
            {
                var monthSummary = new MonthSummary
                {
                    Id = vm.Id.ToUUID(),
                    When = vm.When.ToTimestamp(),
                    IsExpanded = vm.IsExpanded,
                    UncategorizedCount = vm._payments.Count(v => v.CategoryId == null && v.DebtId == null), // TODO replace linq with field
                    Summary =
                    {
                        // TODO replace linq with field
                        vm._payments.GroupBy(v => v.Ccy).Select(v => new CurrencySummary {Amount = v.Sum(t => t.Amount), Currency = v.Key})
                    }
                };

                // TODO replace linq with field
                var visiblePayments = vm._payments.Where(v => v.DebtId != null || v.CategoryId != null || vm.ShowUncategorized);

                if (vm.IsExpanded)
                {
                    foreach (var v in visiblePayments.OrderByDescending(v => v.When))
                    {
                        monthSummary.Payments.Add(new PaymentView {Payment = ToPaymentView(v)});
                    }
                }

                return monthSummary;
            }

            public static Payment ToPaymentView(PaymentModel vm)
            {
                return new Payment
                {
                    Id = vm.Id.ToUUID(),
                    Amount = vm.Amount,
                    Ccy = vm.Ccy,
                    Kind = vm.Kind,
                    What = vm.What,
                    CategoryId = vm.CategoryId.ToUUID(),
                    When = vm.When.ToTimestamp(),
                    ColumnId = vm.ColumnId.ToUUID(),
                    DebtId = vm.DebtId.ToUUID()
                };
            }

            public static string GetKey(PaymentModel paymentModel) => paymentModel.When.Year + "" + paymentModel.When.Month;
        }

        public class PaymentsMonthViewModel : ViewModelBase
        {
            private readonly PaymentsViewModel _owner;
            internal List<PaymentModel> _payments;

            public PaymentsMonthViewModel(PaymentsViewModel owner, DateTime when, string key)
            {
                Id = Guid.NewGuid();
                Key = key;
                _owner = owner;
                When = when;
                _payments = new List<PaymentModel>();
                IsExpanded = When.AddMonths(3) > DateTime.Now;
            }

            public Guid Id { get; }

            public DateTime When { get; }
            
            public bool IsExpanded { get; }
        
            public bool ShowUncategorized { get; private set; }

            public string Key { get; }
        
            public int Count => _payments.Count;
        
            public void UpdateShowCategorized(bool requestShowCategorized)
            {
                if (ShowUncategorized != requestShowCategorized)
                {
                    ShowUncategorized = requestShowCategorized;
                    // TODO calculate everything
                }
            }

            public void AddPayment(PaymentModel paymentModel)
            {
                _payments.Add(paymentModel);
                // TODO recalculate payments
            }

            public void RemovePayment(PaymentModel paymentModel)
            {
                _payments.Remove(paymentModel);
                // TODO recalculate payments, send update
            }

            public void UpdatePayment(PaymentModel pm)
            {
                _owner.SendUpdate(new PaymentsStream
                {
                    Updated = new PaymentViewUpdate
                    {
                        Id = { Id.ToUUID() },
                        Position = _payments.IndexOf(pm),
                        View = new PaymentView
                        {
                            Payment = PaymentTransformers.ToPaymentView(pm)
                        }
                    }
                });
                // TODO move to correct group
                // TODO recalculate uncategorized count / summmary
            }

            public void ExpandCollapseGroup(IEnumerable<Guid> requestRowNumber)
            {
                if (!requestRowNumber.Any())
                {
                    // TODO switch isExpanded;
                }
                else
                {
                    // TODO delegate to payment group
                }
            }
        }

        private ServerObservableCollection<PaymentsMonthViewModel, PaymentsStream> _collection;

        public PaymentsViewModel(ObjectRepository objectRepository, ILogger<PaymentsViewModel> logger) : base(objectRepository, logger)
        {
        }

        protected override Task Init()
        {
            _collection = new ServerObservableCollection<PaymentsMonthViewModel, PaymentsStream>(SendUpdate, PaymentTransformers.OnAdd, PaymentTransformers.OnRemove, PaymentTransformers.OnUpdate);
            
            _collection.SendUpdates = false;
            
            foreach (PaymentModel v in ObjectRepository.Set<PaymentModel>())
            {
                AddPayment(v);
            }

            var paymentsList = new PaymentsList
            {
                ShowCategorized = ShowCategorized,
            };
            foreach (var v in _collection)
            {
                var monthSummary = PaymentTransformers.ToMonthSummary(v);
                paymentsList.Payments.Add(new PaymentView {Summary = monthSummary});
            }
            
            SendUpdate(new PaymentsStream
            {
                Snapshot = paymentsList
            });

            _collection.SendUpdates = true;

            return Task.CompletedTask;
        }
        
        public bool ShowCategorized { get; private set; }

        private void AddPayment(PaymentModel paymentModel)
        {
            var key = PaymentTransformers.GetKey(paymentModel);
            var existing = _collection.FirstOrDefault(v => v.Key == key);
            if (existing == null)
            {
                existing = new PaymentsMonthViewModel(this, paymentModel.When.Date, key);
                int i;
                for (i = 0; i < _collection.Count; i++)
                {
                    if (String.Compare(_collection[i].Key, key, StringComparison.Ordinal) < 0)
                    {
                        break;
                    }
                }
                _collection.Insert(i, existing);
            }

            existing.AddPayment(paymentModel);
        }

        private void RemovePayment(PaymentModel paymentModel)
        {
            var key = PaymentTransformers.GetKey(paymentModel);
            var matchingMonth = _collection.First(v => v.Key == key);

            matchingMonth.RemovePayment(paymentModel);
            if (matchingMonth.Count == 0)
            {
                _collection.Remove(matchingMonth);
            }
        }

        private void UpdatePayment(PaymentModel pm)
        {
            // payment date can't be edited, thus it is safe to delegate only to matching month;
            var key = PaymentTransformers.GetKey(pm);
            var matchingMonth = _collection.First(v => v.Key == key);
            matchingMonth.UpdatePayment(pm);
        }

        protected override void OnModelRepositoryChanged(ModelChangedEventArgs obj)
        {
            if (obj.Source is PaymentModel pm)
            {
                switch (obj.ChangeType)
                {
                    case ChangeType.Add:
                        AddPayment(pm);
                        break;
                    case ChangeType.Remove:
                        RemovePayment(pm);
                        break;
                    case ChangeType.Update:
                        UpdatePayment(pm);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            base.OnModelRepositoryChanged(obj);
        }

        public void ExpandCollapseGroup(List<Guid> requestRowNumber)
        {
            var matchingMonth = requestRowNumber.First();
            _collection.First(v => v.Id == matchingMonth).ExpandCollapseGroup(requestRowNumber.Skip(1));
        }

        public PaymentDetails GetPaymentDetails(UUID request)
        {
            throw new NotImplementedException();
        }

        public void UpdateShowCategorized(bool requestShowCategorized)
        {
            ShowCategorized = requestShowCategorized;
            foreach (var monthViewModel in _collection)
            {
                monthViewModel.UpdateShowCategorized(requestShowCategorized);
            }
        }

        public void DeletePayment(UUID request)
        {
            var id = request.ToGuid();
            var payment = ObjectRepository.Set<PaymentModel>().First(v => v.Id == id);
            if (payment.Sms != null)
            {
                payment.Sms.AppliedRule = null;
            }

            ObjectRepository.Remove(payment);
        }

        public void EditPayment(Payment request)
        {
            var id = request.Id.ToGuid();
            var payment = ObjectRepository.Set<PaymentModel>().First(v => v.Id == id);

            payment.Amount = request.Amount;
            payment.Ccy = request.Ccy;
            payment.What = request.What;
            payment.Category = null;
            payment.Kind = request.Kind;
            payment.Category = request.CategoryId == null ? null : ObjectRepository.Set<SpentCategoryModel>().Find(request.CategoryId.ToGuid());
            payment.Column = request.ColumnId == null ? null : ObjectRepository.Set<MoneyColumnMetadataModel>().Find(request.ColumnId.ToGuid());
            payment.Debt = request.DebtId == null ? null : ObjectRepository.Set<DebtModel>().Find(request.DebtId.ToGuid());
            payment.UserEdited = true;

            new SpentCategoryProcessor(ObjectRepository).Process();
        }

        public void SplitPayment(SplitPaymentRequest request)
        {
            var amount = request.Amount;
            var id = request.Id.ToGuid();
            var payment = ObjectRepository.Set<PaymentModel>().First(v => v.Id == id);

            var newKind = amount < 0 ? PaymentKind.Expense : PaymentKind.Income;

            if (payment.Kind != PaymentKind.Expense && payment.Kind != PaymentKind.Income)
            {
                newKind = payment.Kind;

                payment.Amount -= amount;
            }
            else
            {
                amount = Math.Abs(amount);

                if (payment.Kind == newKind.GetOpposite())
                {
                    amount = -amount;
                }
                
                payment.Amount -= amount;

                amount = Math.Abs(amount);
            }

            if (Math.Abs(amount) > 0.01)
            {
                if (payment.Amount < 0 && payment.Kind != payment.Kind.GetOpposite())
                {
                    payment.Kind = payment.Kind.GetOpposite();
                    payment.Amount = Math.Abs(payment.Amount);
                }

                var newPayment = new PaymentModel(payment.When, payment.What, amount, newKind, payment.Ccy, null, payment.Column);
                ObjectRepository.Add(newPayment);
                newPayment.UserEdited = true;
            }
        }
    }
}