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
            Rules = objectRepository.Set<RuleModel>().ToList();
            Categories = objectRepository.Set<SpentCategoryModel>().ToList();
            
            Months = MonthViewModel.FromSms(objectRepository, showHidden).OrderByDescending(v=>v.When).ToList();
        }

        public List<MonthViewModel> Months { get; }

        public IEnumerable<RuleModel> Rules { get; }

        public IEnumerable<SpentCategoryModel> Categories { get; set; }
        public bool ShowHidden { get; set; }
    }
}