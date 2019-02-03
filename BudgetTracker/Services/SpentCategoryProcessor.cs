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
                    .Aggregate(new Dictionary<string, PaymentModel>(), (a, b) =>
                    {
                        a[b.What] = b;
                        return a;
                    });
                
                var regexOptions = RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase;
                var cats = _objectRepository.Set<SpentCategoryModel>().Where(v => !string.IsNullOrWhiteSpace(v.Pattern))
                    .ToDictionary(v => v, v => new Regex(v.Pattern, regexOptions));
                var debs = _objectRepository.Set<DebtModel>().Where(v => !string.IsNullOrWhiteSpace(v.RegexForTransfer))
                    .ToDictionary(v => v, v => new Regex(v.RegexForTransfer, regexOptions));

                foreach (var p in payments)
                {
                    if (matchedPayments.ContainsKey(p.What) && !string.IsNullOrWhiteSpace(p.What))
                    {
                        p.Debt = matchedPayments[p.What].Debt;
                        p.Category = matchedPayments[p.What].Category;
                        
                        if (p.Category.Kind != PaymentKind.Unknown)
                        {
                            p.Kind = p.Category.Kind;
                        }
                        continue;
                    }
                    
                    foreach (var category in cats)
                    {
                        if (string.Equals(category.Key.Category, p.What) || category.Value.IsMatch(p.What))
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
                        if (d.Value.IsMatch(p.What))
                        {
                            p.Debt = d.Key;
                            break;
                        }
                    }
                }
            }
        }
    }
}