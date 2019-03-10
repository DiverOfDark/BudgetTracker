using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BudgetTracker.Model;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Services
{
    public class SpentCategoryProcessor
    {
        private readonly ObjectRepository _objectRepository;

        public SpentCategoryProcessor(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        public void Process()
        {
            lock (typeof(SpentCategoryProcessor))
            {
                var payments = _objectRepository.Set<PaymentModel>().Where(v => v.Category == null && v.Debt == null).ToList();

                var matchedPayments = _objectRepository.Set<PaymentModel>()
                    .Where(v => v.Category != null || v.Debt != null)
                    .OrderBy(v => v.When)
                    .GroupBy(v => v.What)
                    .Where(v => v.GroupBy(s => s.Category).Count() == 1)
                    .Aggregate(new Dictionary<string, PaymentModel>(), (a, b) =>
                    {
                        a[b.Key] = b.First();
                        return a;
                    });
                
                var regexOptions = RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase;
                var cats = _objectRepository.Set<SpentCategoryModel>().Where(v => !string.IsNullOrWhiteSpace(v.Pattern))
                    .ToDictionary(v => v, v => new Regex(v.Pattern, regexOptions));
                var debs = _objectRepository.Set<DebtModel>().Where(v => !string.IsNullOrWhiteSpace(v.RegexForTransfer))
                    .ToDictionary(v => v, v => new Regex(v.RegexForTransfer, regexOptions));

                foreach (var p in payments)
                {
                    foreach (var category in cats)
                    {
                        var stringContains = !string.IsNullOrWhiteSpace(category.Key.Pattern) && p.What.Contains(category.Key.Pattern);
                        var categoryNameSameAsTransferDescription = string.Equals(category.Key.Category, p.What);
                        var regexMatch = category.Value.IsMatch(p.What);
                        if (stringContains || categoryNameSameAsTransferDescription || regexMatch)
                        {
                            p.Category = category.Key;

                            if (category.Key.Kind != PaymentKind.Unknown)
                            {
                                p.Kind = category.Key.Kind;
                            }

                            break;
                        }
                    }

                    foreach (var d in debs)
                    {
                        var stringContains = !string.IsNullOrWhiteSpace(d.Key.RegexForTransfer) && p.What.Contains(d.Key.RegexForTransfer);
                        var regexMatch = d.Value.IsMatch(p.What);
                        if (stringContains || regexMatch)
                        {
                            p.Debt = d.Key;
                            break;
                        }
                    }
                    
                    if (matchedPayments.ContainsKey(p.What) && !string.IsNullOrWhiteSpace(p.What))
                    {
                        p.Debt = matchedPayments[p.What].Debt;
                        p.Category = matchedPayments[p.What].Category;
                        
                        continue;
                    }
                }
            }
        }
    }
}