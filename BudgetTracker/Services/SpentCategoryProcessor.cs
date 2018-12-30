using System.Linq;
using System.Text.RegularExpressions;
using BudgetTracker.Model;

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
                var categories = _objectRepository.Set<SpentCategoryModel>();
                var debts = _objectRepository.Set<DebtModel>();

                var regexOptions = RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase;
                var cats = categories.ToDictionary(v => v, v => new Regex(v.Pattern, regexOptions));
                foreach (var p in payments)
                {
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
                }

                var debs = debts.ToDictionary(v => v, v => new Regex(v.RegexForTransfer, regexOptions));
                foreach (var p in payments)
                {
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