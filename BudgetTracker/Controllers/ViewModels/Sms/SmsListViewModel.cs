using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Sms
{
    public class SmsListViewModel
    {
        public SmsListViewModel(ObjectRepository objectRepository, bool showHidden)
        {
            ShowHidden = showHidden;
            
            Months = SmsMonthViewModel.FromSms(objectRepository, showHidden).OrderByDescending(v=>v.When).ToList();
        }

        public List<SmsMonthViewModel> Months { get; }

        public bool ShowHidden { get; set; }
    }
}