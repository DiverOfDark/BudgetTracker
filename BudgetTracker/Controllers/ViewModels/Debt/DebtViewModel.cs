using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Debt
{
    public class DebtViewModel
    {
        public DebtViewModel(DebtModel model)
        {
            Model = model;
        }

        public DebtModel Model { get; }

        public double Returned => Model.Payments.Where(v=>v.Kind == PaymentKind.Income).Select(s => s.Amount).Sum();
        
        public double AmountWithPercentage => Model.Amount * (1 + Model.Percentage / 100);
        public double Remaining => Model.Amount - Returned;
        public double RemainingWithPercentage => AmountWithPercentage - Returned;
    }
}