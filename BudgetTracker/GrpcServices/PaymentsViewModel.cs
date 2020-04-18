using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetTracker.Model;
using BudgetTracker.Services;
using Microsoft.Extensions.Logging;

namespace BudgetTracker.GrpcServices
{
    public class PaymentsViewModel : GrpcViewModelBase<PaymentsStream>
    {
        public PaymentsViewModel(ObjectRepository objectRepository, ILogger<PaymentsViewModel> logger) : base(objectRepository, logger)
        {
        }

        protected override Task Init()
        {
            return Task.CompletedTask;
        }

        /*
 public class PaymentViewModel 
    {
        public PaymentViewModel(PaymentModel paymentModel)
        {
            When = paymentModel.When;
            Amount = paymentModel.Amount;
            Ccy = paymentModel.Ccy;
            What = paymentModel.What;
            Id = paymentModel.Id;
            Provider = paymentModel.Column?.Provider;
            Account = paymentModel.Column?.AccountName;
            Kind = paymentModel.Kind.GetDisplayName();
            KindId = (int) paymentModel.Kind;

            CategoryId = paymentModel.CategoryId;
            Category = paymentModel.Category?.Category;
            DebtId = paymentModel.Debt?.Id;
            Debt = paymentModel.Debt?.Description;
            ColumnId = paymentModel.Column?.Id;
            StatementReference = paymentModel.StatementReference;
            Sms = paymentModel.Sms?.Message;
        }

        public string What { get; }

        public string Ccy { get; }

        public Guid Id { get; } 

        public double Amount { get; }

        public DateTime When { get; }
        
        public int KindId { get; }
        public string Kind { get; }
        public string Provider { get; }
        public string Account { get; }

        public string StatementReference { get; }
        
        public string Sms { get; }
        
        public Guid? CategoryId { get; set; }
        public Guid? DebtId { get; set; }
        public Guid? ColumnId { get; set; }
        public string Category { get; }
        public string Debt { get; }
    }
            [HttpPost]
        public OkResult SplitPayment(Guid id, double amount)
        {
            
            
            return Ok();
        }
             */
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