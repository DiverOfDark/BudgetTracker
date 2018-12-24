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
        public double AmountWithPercentage => Model.Amount * (1 + Model.Percentage / 100);
        public double Remaining => Model.Amount - Model.Returned;
        public double RemainingWithPercentage => AmountWithPercentage - Model.Returned;
    }
}