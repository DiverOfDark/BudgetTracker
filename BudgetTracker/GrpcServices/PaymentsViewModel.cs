using System;
using System.Collections;
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
        internal abstract class RowViewModel : IEnumerable<PaymentView>
        {
            public abstract IEnumerator<PaymentView> GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            
            protected static double OrderingComparator(PaymentModel arg, Ordering ordering)
            {
                if (ordering == Ordering.Amount) return arg.Amount;
                if (ordering == Ordering.Date) return arg.When.Ticks;
                return 0;
            }

            protected static double OrderingComparator(RowViewModel arg, Ordering ordering)
            {
                if (arg is PaymentRowViewModel prvm)
                    return OrderingComparator(prvm, ordering);
                if (arg is PaymentGroupViewModel pgvm)
                    return OrderingComparator(pgvm, ordering);
                return 0;
            }            

            protected static double OrderingComparator(PaymentRowViewModel arg, Ordering ordering)
            {
                if (ordering == Ordering.Amount) return arg.Model.Amount;
                if (ordering == Ordering.Date) return arg.Model.When.Ticks;
                return 0;
            }

            protected static double OrderingComparator(PaymentGroupViewModel arg, Ordering ordering)
            {
                if (ordering == Ordering.Amount) return arg.Amount;
                if (ordering == Ordering.Date) return arg.When.Ticks;
                return 0;
            }
        }

        internal class PaymentRowViewModel : RowViewModel
        {
            private readonly PaymentsViewModel _owner;
            public PaymentModel Model { get; }

            public PaymentRowViewModel(PaymentsViewModel owner, PaymentModel model)
            {
                _owner = owner;
                Model = model;
            }
            
            public PaymentDetails GetDetails()
            {
                return new PaymentDetails
                {
                    Payment = ToPayment(),
                    Sms = Model.Sms?.Message ?? "",
                    StatementReference = Model.StatementReference ?? ""
                };
            }

            public override IEnumerator<PaymentView> GetEnumerator()
            {
                yield return new PaymentView {Payment = ToPayment()};
            }

            private Payment ToPayment()
            {
                return new Payment
                {
                    Account = Model.Column.AccountName,
                    Amount = Model.Amount,
                    Category = Model.Category?.Category ?? "",
                    CategoryId = Model.CategoryId.ToUUID(),
                    Ccy = Model.Ccy,
                    Debt = Model.Debt?.Description ?? "",
                    Id = Model.Id.ToUUID(),
                    Kind = Model.Kind,
                    Provider = Model.Column.Provider,
                    What = Model.What,
                    When = Model.When.ToTimestamp(),
                    ColumnId = Model.ColumnId.ToUUID(),
                    DebtId = Model.DebtId.ToUUID(),
                };
            }

            public void UpdatePayment(ModelChangedEventArgs changedEventArgs)
            {
                // TODO send update
            }
        }

        internal class PaymentGroupViewModel : RowViewModel
        {
            private readonly PaymentsViewModel _owner;

            public PaymentGroupViewModel(PaymentsViewModel owner)
            {
                _owner = owner;
                Payments = new List<PaymentRowViewModel>();
            }

            public double Amount => Payments.Sum(v => v.Model.Amount);

            public DateTime When => Payments.First().Model.When;

            public List<PaymentRowViewModel> Payments { get; }

            public bool IsExpanded { get; } = false;

            private static string OneOrDefault(IEnumerable<string> distinct)
            {
                string currentValue = null;
                foreach (var v in distinct)
                {
                    if (currentValue == null)
                    {
                        currentValue = v;
                    }
                    else
                    {
                        return "";
                    }
                }

                return currentValue;
            }
            
            public override IEnumerator<PaymentView> GetEnumerator()
            {
                yield return new PaymentView
                {
                    Group = new PaymentGroup
                    {
                        PaymentCount = Payments.Count,
                        IsExpanded = IsExpanded,
                        When = When.ToTimestamp(),
                        What = OneOrDefault(Payments.Select(v => v.Model.What).Distinct()),
                        Account = OneOrDefault(Payments.Select(v => v.Model.Column.AccountName).Distinct()),
                        Amount = Amount,
                        Category = OneOrDefault(Payments.Select(v => v.Model.Category?.Category).Distinct()),
                        Ccy = Payments.First().Model.Ccy,
                        Kind = Payments.First().Model.Kind,
                        Provider = OneOrDefault(Payments.Select(v => v.Model.Column.Provider).Distinct())
                    }
                };
                
                if (IsExpanded)
                {
                    foreach (var view in Payments.SelectMany(v=>v))
                    {
                        yield return view;
                    }
                }
            }

            public void AddPayment(PaymentModel paymentModel)
            {
                int idx;
                
                for (idx = 0; idx < Payments.Count; idx++)
                {
                    var newComparator = OrderingComparator(paymentModel, _owner.Ordering);
                    var oldComparator = OrderingComparator(Payments[idx], _owner.Ordering);

                    if (oldComparator > newComparator)
                        break;
                }

                Payments.Insert(idx, new PaymentRowViewModel(_owner, paymentModel));
                // TODO send update
            }

            public void RemovePayment(PaymentModel paymentModel)
            {
                foreach (var payment in Payments)
                {
                    if (payment.Model == paymentModel)
                    {
                        Payments.Remove(payment);
                        // TODO send update with removal
                        break;
                    }
                }
            }

            public void ExpandCollapseGroup()
            {
                throw new NotImplementedException();
            }
        }

        private class MonthViewModel : RowViewModel
        {
            private readonly PaymentsViewModel _owner;
            private readonly MonthSummary _summary;

            public MonthViewModel(PaymentsViewModel owner, DateTime matchingMonth)
            {
                _owner = owner;
                When = matchingMonth;
                _summary = new MonthSummary
                {
                    IsExpanded = (DateTime.Now - matchingMonth).TotalDays < 31 * 3, // by default expand only last 3 month
                    When = matchingMonth.ToTimestamp(),
                    UncategorizedCount = 0,
                };
                Payments = new List<RowViewModel>();
            }

            public DateTime When { get; }

            public List<RowViewModel> Payments { get; }

            public override IEnumerator<PaymentView> GetEnumerator()
            {
                yield return new PaymentView
                {
                    Summary = _summary
                };

                if (_summary.IsExpanded)
                {
                    foreach (var v in Payments.SelectMany(v=>v))
                    {
                        yield return v;
                    }
                }
            }
            
            public void AddPayment(PaymentModel paymentModel)
            {
                _summary.UncategorizedCount += paymentModel.CategoryId.HasValue ? 0 : 1;
                var ccySummary = _summary.Summary.FirstOrDefault(v => v.Currency == paymentModel.Ccy);
                if (ccySummary == null)
                {
                    _summary.Summary.Add(ccySummary = new CurrencySummary{ Amount = 0, Currency = paymentModel.Ccy });
                }

                ccySummary.Amount += paymentModel.Amount;
               
                // TODO send updated summary

                string groupingFunc(PaymentModel pm) => pm.Kind + pm.Ccy + pm.What;

                var inserterGrouper = groupingFunc(paymentModel);
                
                int idx;
                for (idx = 0; idx < Payments.Count; idx++)
                {
                    var current = OrderingComparator(Payments[idx], _owner.Ordering);
                    var next = OrderingComparator(paymentModel, _owner.Ordering);

                    if (Payments[idx] is PaymentGroupViewModel pgvm)
                    {
                        var existingGrouper = groupingFunc(pgvm.Payments.First().Model);
                        if (existingGrouper == inserterGrouper)
                        {
                            pgvm.AddPayment(paymentModel);
                            return;
                        }
                    } else if (Payments[idx] is PaymentRowViewModel prvm)
                    {
                        var existingGrouper = groupingFunc(prvm.Model);
                        if (existingGrouper == inserterGrouper)
                        {
                            Payments.RemoveAt(idx);
                            var group = new PaymentGroupViewModel(_owner);
                            // TODO send update about row removal
                            Payments.Insert(idx, group);
                            // TODO send update about row addition
                            group.AddPayment(prvm.Model);
                            group.AddPayment(paymentModel);
                            return;

                            // TODO remove Payments[idx]
                            // TODO insert in its place PaymentsGroup of Payments[idx] and payment
                            // TODO return
                        }
                    }
                    

                    if (current > next)
                    {
                        break;
                    }
                }

                Payments.Insert(idx, new PaymentRowViewModel(_owner, paymentModel));
                // TODO notify inserted payment
            }

            public void RemovePayment(PaymentModel paymentModel)
            {
                _summary.UncategorizedCount -= paymentModel.CategoryId.HasValue ? 0 : 1;
                var matchingSummary = _summary.Summary.First(v => v.Currency == paymentModel.Ccy);
                matchingSummary.Amount -= paymentModel.Amount;
                if (Math.Abs(matchingSummary.Amount) < double.Epsilon)
                {
                    _summary.Summary.Remove(matchingSummary);
                }

                // TODO send updated summary

                foreach (var v in Payments)
                {
                    if (v is PaymentRowViewModel prvm && prvm.Model == paymentModel)
                    {
                        Payments.Remove(prvm);
                        // TODO send update with removed row;
                        break;
                    }

                    if (v is PaymentGroupViewModel pgvm)
                    {
                        pgvm.RemovePayment(paymentModel);

                        if (pgvm.Payments.Count == 1)
                        {
                            // TODO remove paymentgroup 
                            // TODO send update with removal of payment group
                            // TODO add paymentrow instead in that place
                            // TODO send update with addition of payment row
                        }
                    }
                }
            }

            public void UpdatePayment(PaymentModel paymentModel, ModelChangedEventArgs changedEventArgs)
            {
                if (changedEventArgs.PropertyName == nameof(PaymentModel.CategoryId))
                {
                    // TODO update summary                    
                }

                if (changedEventArgs.PropertyName == nameof(PaymentModel.Amount))
                {
                    // TODO update summary                    
                }

                if (changedEventArgs.PropertyName == nameof(PaymentModel.Ccy))
                {
                    // TODO update summary                    
                }

                // TODO send updated summary row
                
                foreach (var row in Payments)
                {
                    if (row is PaymentGroupViewModel paymentGroupViewModel)
                    {
                        foreach (var payment in paymentGroupViewModel.Payments)
                        {
                            if (payment.Model == paymentModel)
                            {
                                payment.UpdatePayment(changedEventArgs);
                            }
                        }
                    }

                    if (row is PaymentRowViewModel paymentRowViewModel)
                    {
                        if (paymentRowViewModel.Model == paymentModel)
                        {
                            paymentRowViewModel.UpdatePayment(changedEventArgs);
                        }
                    }
                }
            }

            public void UpdateShowCategorized(bool value)
            {
                throw new NotImplementedException();
            }

            public void UpdateOrdering(Ordering value)
            {
                throw new NotImplementedException();
            }

            public void ExpandCollapseGroup(int requestRowNumber)
            {
                int count = 0;
                foreach (var payment in Payments)
                {
                    if (count == requestRowNumber)
                    {
                        ((PaymentGroupViewModel) payment).ExpandCollapseGroup();
                        break;
                    }
                    count += payment.Count();
                }
            }

            public void ExpandCollapseMonth()
            {
                if (!_summary.IsExpanded)
                {
                    _summary.IsExpanded = true;
                    // ToDo send update with inserts of rows
                }
                else
                {
                    _summary.IsExpanded = false;
                    // ToDo send update with removes of rows
                }
            }
        }

        internal class RootRowsViewModel : RowViewModel
        {
            private readonly PaymentsViewModel _owner;

            private readonly List<MonthViewModel> _months;

            public RootRowsViewModel(PaymentsViewModel owner)
            {
                _owner = owner;
                _months = new List<MonthViewModel>();
            }

            public override IEnumerator<PaymentView> GetEnumerator()
            {
                foreach (var v in _months.SelectMany(v => v))
                {
                    yield return v;
                }
            }

            public void AddPayment(PaymentModel paymentModel)
            {
                var matchingMonth = new DateTime(paymentModel.When.Year, paymentModel.When.Month, 1);
                var matchingMonthViewModel = _months.FirstOrDefault(v => v.When == matchingMonth);
                if (matchingMonthViewModel == null)
                {
                    matchingMonthViewModel = new MonthViewModel(_owner, matchingMonth);
                    int idx;
                    for (idx = 0; idx < _months.Count; idx++)
                    {
                        if (matchingMonth > _months[idx].When)
                            break;
                    }

                    _months.Insert(idx, matchingMonthViewModel);
                    // TODO send inserted month row view
                }
                
                matchingMonthViewModel.AddPayment(paymentModel);
            }

            public void RemovePayment(PaymentModel pm)
            {
                foreach (var m in _months)
                {
                    m.RemovePayment(pm);
                    if (!m.Any())
                    {
                        _months.Remove(m);
                        // TODO send removed month row view
                    }
                }
            }
            
            public void UpdatePayment(PaymentModel pm, ModelChangedEventArgs changedEventArgs)
            {
                foreach (var m in _months)
                {
                    m.UpdatePayment(pm, changedEventArgs);
                }
            }

            public void UpdateShowCategorized(bool value)
            {
                foreach (var month in _months)
                {
                    month.UpdateShowCategorized(value);
                }
            }

            public void UpdateOrdering(Ordering value)
            {
                foreach (var month in _months)
                {
                    month.UpdateOrdering(value);
                }
            }

            public void ExpandCollapseGroup(Timestamp requestMonth, int requestRowNumber)
            {
                var dt = requestMonth.ToDateTime();
                var matchingMonth = _months.First(v => v.When == dt);
                matchingMonth.ExpandCollapseGroup(requestRowNumber);
            }

            public void ExpandCollapseMonth(Timestamp @when)
            {
                var dt = when.ToDateTime();
                var matchingMonth = _months.First(v => v.When == dt);
                matchingMonth.ExpandCollapseMonth();
            }
        }
        
        private bool _showCategorized = true;
        private Ordering _ordering = Ordering.Amount;

        private RootRowsViewModel _state;

        public PaymentsViewModel(ObjectRepository objectRepository, ILogger<PaymentsViewModel> logger) : base(objectRepository, logger)
        {
        }

        public bool ShowCategorized
        {
            get => _showCategorized;
            set
            {
                if (value != _showCategorized)
                {
                    _showCategorized = value;
                    _state?.UpdateShowCategorized(value);
                }
            }
        }

        public Ordering Ordering
        {
            get => _ordering;
            set
            {
                if (value != _ordering)
                {
                    _ordering = value;
                    _state?.UpdateOrdering(value);
                }
            }
        }

        protected override Task Init()
        {
            _state = new RootRowsViewModel(this);
            foreach (PaymentModel paymentModel in ObjectRepository.Set<PaymentModel>())
            {
                _state.AddPayment(paymentModel);
            }
            SendSnapshot();
            return Task.CompletedTask;
        }

        private void SendSnapshot()
        {
            SendUpdate(new PaymentsStream
            {
                Snapshot = new PaymentsList
                {
                    Ordering = Ordering,
                    ShowCategorized = ShowCategorized,
                    Payments = {_state.ToList()}
                }
            });
        }

        protected override void OnModelRepositoryChanged(ModelChangedEventArgs obj)
        {
            if (obj.Source is PaymentModel pm)
            {
                if (obj.ChangeType == ChangeType.Add)
                {
                    _state.AddPayment(pm);
                }

                if (obj.ChangeType == ChangeType.Remove)
                {
                    _state.RemovePayment(pm);
                }

                if (obj.ChangeType == ChangeType.Update)
                {
                    _state.UpdatePayment(pm, obj);
                }
            }
        }

        public void ExpandCollapseGroup(Timestamp requestMonth, int requestRowNumber) => _state.ExpandCollapseGroup(requestMonth, requestRowNumber);

        public void ExpandCollapseMonth(Timestamp when) => _state.ExpandCollapseMonth(when);

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

        public PaymentDetails GetPaymentDetails(UUID request)
        {
            var pm = ObjectRepository.Set<PaymentModel>().Find(request.ToGuid());

            return new PaymentRowViewModel(this, pm).GetDetails();
        }
    }
}