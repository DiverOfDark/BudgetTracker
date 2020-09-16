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
            public static PaymentsStream OnAdd(PaymentsMonthViewModel arg1, int position) =>
                new PaymentsStream
                {
                    Added = new PaymentViewUpdate
                    {
                        Position = position,
                        View = arg1.ToMonthSummary()
                    }
                };

            public static PaymentsStream OnRemove(PaymentsMonthViewModel arg1, int position) =>
                new PaymentsStream
                {
                    Removed = new PaymentViewUpdate
                    {
                        Position = position
                    }
                };

            public static PaymentsStream OnUpdate(PaymentsMonthViewModel arg1, int position) =>
                new PaymentsStream
                {
                    Updated = new PaymentViewUpdate
                    {
                        Position = position,
                        View = arg1.ToMonthSummary()
                    }
                };

            public static Payment ToPaymentView(PaymentModel vm)
            {
                var paymentView = new Payment
                {
                    Id = vm.Id.ToUUID(),
                    Amount = vm.Amount,
                    Ccy = vm.Ccy,
                    Kind = vm.Kind,
                    What = vm.What,
                    CategoryId = vm.CategoryId.ToUUID(),
                    When = vm.When.ToTimestamp(),
                    ColumnId = vm.ColumnId.ToUUID(),
                    DebtId = vm.DebtId.ToUUID(),
                    Statement = vm.StatementReference ?? ""
                };
                if (vm.Sms != null)
                {
                    paymentView.Sms = vm.Sms.Message;
                }
                return paymentView;
            }

            public static string GetKey(PaymentModel paymentModel) => paymentModel.When.Year + "" + paymentModel.When.Month;

            public static PaymentGroup ToPaymentGroup(PaymentsGroupViewModel vm, bool showCategorized)
            {
                var visiblePayments = vm.GetVisiblePayments(showCategorized);
                var paymentGroup = new PaymentGroup
                {
                    IsExpanded = vm.IsExpanded,
                    Amount = visiblePayments.Select(v=>v.Amount).Sum(),
                    Ccy = visiblePayments[0].Ccy,
                    Id = vm.Id.ToUUID(),
                    Kind = visiblePayments[0].Kind,
                    PaymentCount = visiblePayments.Count,
                    What = visiblePayments[0].What,
                    When = visiblePayments[0].When.ToTimestamp(),
                    CategoryId = visiblePayments[0].CategoryId.ToUUID(),
                    ColumnId = visiblePayments[0].ColumnId.ToUUID(),
                    DebtId = visiblePayments[0].DebtId.ToUUID()
                };
                if (vm.IsExpanded)
                {
                    foreach (var v in visiblePayments)
                    {
                        paymentGroup.Payments.Add(new PaymentView {Payment = ToPaymentView(v)});
                    }
                }
                return paymentGroup;
            }
        }

        public class PaymentsGroupViewModel : ViewModelBase
        {
            public PaymentsGroupViewModel(PaymentModel model)
            {
                Id = Guid.NewGuid();
                Payments.Add(model);
            }
            
            public Guid Id { get; }
            
            public bool IsExpanded { get; set; }
            
            public List<PaymentModel> Payments { get; } = new List<PaymentModel>();

            public List<PaymentModel> GetVisiblePayments(bool showCategorized) => Payments.Where(v => v.DebtId != null || v.CategoryId != null || !showCategorized).OrderByDescending(v => v.When).ToList();
        }

        public class PaymentsMonthViewModel : ViewModelBase
        {
            private readonly Dictionary<string, PaymentsGroupViewModel> _payments;

            public PaymentsMonthViewModel(DateTime when, string key)
            {
                Id = Guid.NewGuid();
                Key = key;
                When = when;
                _payments = new Dictionary<string, PaymentsGroupViewModel>();
                IsExpanded = When.AddDays(45) > DateTime.Now;
            }
            
            public Guid Id { get; }

            public DateTime When { get; }
            
            public bool IsExpanded { get; set; }
        
            public bool ShowCategorized { get; set; }

            public string Key { get; }
            public int Count => _payments.Values.Select(v => v.Payments.Count).Sum();

            private string PaymentKey(PaymentModel model) => model.Kind + model.What;
            
            public void AddPayment(PaymentModel paymentModel)
            {
                var key = PaymentKey(paymentModel);
                if (!_payments.ContainsKey(key))
                {
                    _payments[key] = new PaymentsGroupViewModel(paymentModel);
                }
                else
                {
                    _payments[key].Payments.Add(paymentModel);
                }
            }

            public void RemovePayment(PaymentModel paymentModel)
            {
                var key = PaymentKey(paymentModel);

                var matchingList = _payments[key];

                matchingList.Payments.Remove(paymentModel);
                if (matchingList.Payments.Count == 0)
                    _payments.Remove(key);
            }

            public void UpdateExpanded(Guid guid)
            {
                var vm = _payments.Values.First(v=>v.Id == guid);
                vm.IsExpanded = !vm.IsExpanded;
            }

            public MonthSummary ToMonthSummary()
            {
                var monthSummary = new MonthSummary
                {
                    Id = Id.ToUUID(),
                    When = When.ToTimestamp(),
                    IsExpanded = IsExpanded,
                    UncategorizedCount = _payments.Values.SelectMany(v=>v.Payments).Count(v => v.CategoryId == null && v.DebtId == null), 
                    Summary =
                    {
                        _payments.Values.SelectMany(v=>v.Payments).GroupBy(v => v.Ccy).Select(v => new CurrencySummary {Amount = v.Sum(t => t.Amount), Currency = v.Key})
                    }
                };

                var visiblePayments = _payments.Values
                    .OrderByDescending(v => v.GetVisiblePayments(ShowCategorized).Select(t=>t.When).Concat(new[]{DateTime.MinValue}).Max())
                    .ToList();

                if (IsExpanded)
                {
                    foreach (var group in visiblePayments)
                    {
                        var matchingPayments = group.GetVisiblePayments(ShowCategorized);

                        if (matchingPayments.Count == 0)
                        {
                            continue;
                        }

                        monthSummary.Payments.Add(matchingPayments.Count == 1
                            ? new PaymentView {Payment = PaymentTransformers.ToPaymentView(matchingPayments[0])}
                            : new PaymentView {Group = PaymentTransformers.ToPaymentGroup(group, ShowCategorized)});
                    }
                }

                return monthSummary;
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

            SendUpdate(new PaymentsStream
            {
                Snapshot = new PaymentsList
                {
                    ShowCategorized = ShowCategorized,
                    Payments = {_collection.Select(v => v.ToMonthSummary())}
                }
            });

            _collection.SendUpdates = true;

            return Task.CompletedTask;
        }
        
        public bool ShowCategorized { get; private set; }

        private void SendMonthUpdate(PaymentsMonthViewModel months)
        {
            if (_collection.SendUpdates)
            {
                SendUpdate(PaymentTransformers.OnUpdate(months, _collection.IndexOf(months)));
            }
        }

        private void AddPayment(PaymentModel paymentModel)
        {
            var key = PaymentTransformers.GetKey(paymentModel);
            var existing = _collection.FirstOrDefault(v => v.Key == key);
            if (existing == null)
            {
                existing = new PaymentsMonthViewModel(paymentModel.When.Date, key);
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
            
            SendMonthUpdate(existing);
        }

        private void RemovePayment(PaymentModel paymentModel)
        {
            var key = PaymentTransformers.GetKey(paymentModel);
            var matchingMonth = _collection.First(v => v.Key == key);

            matchingMonth.RemovePayment(paymentModel);
            if (matchingMonth.Count > 0)
            {
                SendMonthUpdate(matchingMonth);
            }
            else
            {
                _collection.Remove(matchingMonth);
            }
        }

        private void UpdatePayment(PaymentModel pm)
        {
            // payment date can't be edited, thus it is safe to delegate only to matching month;
            var key = PaymentTransformers.GetKey(pm);
            var matchingMonth = _collection.First(v => v.Key == key);
            SendMonthUpdate(matchingMonth);
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
            var month = _collection.First(v => v.Id == matchingMonth);
            if (requestRowNumber.Count == 1)
            {
                month.IsExpanded = !month.IsExpanded;
            }
            else
            {
                month.UpdateExpanded(requestRowNumber[1]);
            }

            SendMonthUpdate(month);
        }

        public PaymentDetails GetPaymentDetails(UUID request)
        {
            throw new NotImplementedException();
        }

        public void UpdateShowCategorized(bool requestShowCategorized)
        {
            ShowCategorized = requestShowCategorized;
            _collection.SendUpdates = false;
            foreach (var monthViewModel in _collection)
            {
                monthViewModel.ShowCategorized = requestShowCategorized;
            }

            _collection.SendUpdates = true;
            SendUpdate(new PaymentsStream
            {
                Snapshot = new PaymentsList
                {
                    ShowCategorized = ShowCategorized,
                    Payments = {_collection.Select(v => v.ToMonthSummary())}
                }
            });
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
            SendUpdate(new PaymentsStream
            {
                Snapshot = new PaymentsList
                {
                    ShowCategorized = ShowCategorized,
                    Payments = {_collection.Select(v => v.ToMonthSummary())}
                }
            });
        }
    }
}